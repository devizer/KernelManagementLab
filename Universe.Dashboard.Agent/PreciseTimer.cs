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
            ticker.Start();

            ManualResetEvent waiterStarted = new ManualResetEvent(false);
            Thread waiter = new Thread(_ =>
            {
                tickerStarted.WaitOne();
                waiterStarted.Set();
                while (!Shutdown.WaitOne(0))
                {
                    int kind = WaitHandle.WaitAny(new[] {(WaitHandle)oneSecWaiter, (WaitHandle)Shutdown});
                    if (kind == 0)
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
                        break;
                    }
                }
                
                Console.WriteLine("PreciseTimer has been shut down...");

            }) {IsBackground = true};
            waiter.Start();
            waiterStarted.WaitOne();
        }

        private static void FlushDataSource()
        {
            if (Services == null)
            {
                Console.WriteLine("Skipping Broadcasting: Services are not yet configured");
                return;
            }
            
            Stopwatch sw = Stopwatch.StartNew();
            using (var scope = Services.CreateScope())
            {
                var hubContext = scope.ServiceProvider.GetService<IHubContext<DataSourceHub>>();
                hubContext.Clients.All.SendAsync("ReceiveDataSource", NetStatDataSource.Instance.By_1_Seconds);
            }

#if DEBUG            
            // Console.WriteLine($"DataSource flushed: {sw.Elapsed}" );
#endif
            


        }
    }
    
    public static class ExceptionExtensions
    {
        public static string GetExceptionDigest(this Exception ex)
        {
            List<string> ret = new List<string>();
            while (ex != null)
            {
                ret.Add("[" + ex.GetType().Name + "] " + ex.Message);
                ex = ex.InnerException;
            }

            return string.Join(" --> ", ret);
        }
    }
}