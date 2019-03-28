using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactGraphLab;

namespace Universe.Dashboard.Agent
{
    public static class PreciseTimer
    {
        class Timer
        {
            public ILogger Logger;
            public Action Tick;
            public string Name;
        }

        public static IServiceProvider Services;
        public static readonly ManualResetEvent Shutdown = new ManualResetEvent(false);
        // public static Action AllTheTimerFinished = delegate { };
        static List<Timer> Timers = new List<Timer>();
        private static object SyncTimers = new object();
            
        static PreciseTimer()
        {
            SetupTick();
        }

        public static void AddListener(string name, Action tick)
        {
            var loggerFactory = Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger($"Agent {name}");
            lock (SyncTimers)
                Timers.Add(new Timer
                {
                    Name = name,
                    Logger = logger,
                    Tick = tick,
                });
        }

        // It does never fail ever
        private static void SetupTick()
        {

            AutoResetEvent oneSecWaiter = new AutoResetEvent(false);

            ManualResetEvent tickerStarted = new ManualResetEvent(false);
            Thread ticker = new Thread(_ =>
            {
                tickerStarted.Set();
                while (!Shutdown.WaitOne(0))
                {
                    oneSecWaiter.Set();
                    if (Shutdown.WaitOne(1000))
                        return;
                }
            }) {IsBackground = true};
            ticker.Name = "Agent::Ticker";
            ticker.Start();

            ManualResetEvent waiterStarted = new ManualResetEvent(false);
            Thread waiter = new Thread(_ =>
            {
                tickerStarted.WaitOne();
                waiterStarted.Set();
                while (!Shutdown.WaitOne(0))
                {
                    int indexWaiter = WaitHandle.WaitAny(new[] {(WaitHandle)oneSecWaiter, (WaitHandle)Shutdown});
                    if (indexWaiter == 0)
                    {
                        // TODO: All the timers should be executed in a separate thread.
                        List<Timer> copyOfTimers;
                        lock(SyncTimers) copyOfTimers = new List<Timer>(Timers);
                        foreach (var timer in copyOfTimers)
                        {
                            try
                            {
                                timer.Tick();
                            }
                            catch (Exception ex)
                            {
                                if (timer.Logger != null)
                                {
                                    timer.Logger.LogWarning(
                                        "Background iteration '{agent}' failed: {exception}",
                                        timer.Name,
                                        ex.GetExceptionDigest()
                                    );
                                }
                                else
                                    Console.WriteLine($"Background iteration '{timer.Name}' failed: {ex.GetExceptionDigest()}");
                            }
                        }
                        
                        // It is raised after finish of all the timers
                        // AllTheTimerFinished();

                        FlushDataSource();
                    }
                    else
                    {
                        // Shutdown had been requested
                        break;
                    }
                }
                
                Console.WriteLine("PreciseTimer has been shut down...");

            }) {IsBackground = true};
            waiter.Name = "Agent::Waiter";
            waiter.Start();
            waiterStarted.WaitOne();
        }

        private static void FlushDataSource()
        {
            if (Services == null)
            {
                Console.WriteLine("Skipping Broadcasting: Services are not yet injected");
                return;
            }
            
            Stopwatch sw = Stopwatch.StartNew();
            using (var scope = Services.CreateScope())
            {
                var hubContext = scope.ServiceProvider.GetService<IHubContext<DataSourceHub>>();
                if (hubContext == null)
                {
                    return;
                }

                var localStorage = NetStatDataSource.Instance.By_1_Seconds;
                var broadcastStorage = NetDataSourceView.AsViewModel(localStorage);
                hubContext.Clients.All.SendAsync("ReceiveDataSource", broadcastStorage);
            }

#if DUMPS         
            Console.WriteLine($"DataSource flushed: {sw.Elapsed}" );
#endif
            


        }
    }
}