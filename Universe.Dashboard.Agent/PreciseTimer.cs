using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Universe.Dashboard.Agent
{
    public static class PreciseTimer
    {
        public static IServiceProvider Services;
        public static readonly ManualResetEvent Shutdown = new ManualResetEvent(false);

        class Timer
        {
            public ILogger Logger;
            public Action Tick;
            public string Name;
        }

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
                        foreach (Timer timer in copyOfTimers)
                        {
                            try
                            {
                                var sw = new TempStopwatch();
                                var path = new AdvancedMiniProfilerKeyPath(SharedDefinitions.RootKernelMetricsObserverKey, timer.Name);
                                using (AdvancedMiniProfiler.Step(path))
                                {
                                    timer.Tick();
                                }
#if DEBUG
                                Console.WriteLine($"Timer {timer.Name} took {sw}");
#endif
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

                        using (AdvancedMiniProfiler.Step(SharedDefinitions.RootKernelMetricsObserverKey, "BroadCast"))
                        {
                            FlushDataSource();
                        }
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

        private static long MessageId = 0;
        private static void FlushDataSource()
        {
            if (Services == null)
            {
                Console.WriteLine("Skipping Broadcasting: Services are not yet injected");
                return;
            }

            var sw = new TempStopwatch();
            using (var scope = Services.CreateScope())
            {
                var hubContext = scope.ServiceProvider.GetService<IHubContext<DataSourceHub>>();
                if (hubContext == null)
                {
                    Console.WriteLine("Skipping Broadcasting: IHubContext<DataSourceHub> is not yet injected");
                    return;
                }

                Interlocked.Increment(ref MessageId);

                var netStateStorage = NetStatDataSource.Instance.By_1_Seconds;
                var netStatView = NetDataSourceView.AsViewModel(netStateStorage);
                
                var blockStatStorage = BlockDiskDataSource.Instance.By_1_Seconds;
                var blockStatView = BlockDiskDataSourceView.AsViewModel(blockStatStorage);
                List<DriveDetails> mounts = MountsDataSource.Mounts.FilterForHuman().ToList();

                var swapsOriginal = SwapsDataSource.Swaps;
                var swaps = swapsOriginal.Select(x => new DriveDetails()
                {
                    MountEntry = new MountEntry()
                    {
                        MountPath = $"[priority = {x.Priority}]",
                        Device = x.FileName,
                        FileSystem = "swap",
                    },
                    FreeSpace = (x.Size - x.Used) * 1024,
                    TotalSize = (x.Size) * 1024,
                    IsReady = true,
                    Format = "swap",
                });
                
                mounts.AddRange(swaps);

                // Next line is not thread safe
                var memSummary = MemorySummaryDataSource.Instance.By_1_Seconds.LastOrDefault();
                var memAvailable = memSummary?.Summary.Available;
                var memFree = memSummary?.Summary.Free;
                var hostInfo = new
                {
                    Hostname = Environment.MachineName,
                    Os = HugeCrossInfo.OsDisplayName,
                    Processor = HugeCrossInfo.ProcessorName,
                    Memory = HugeCrossInfo.TotalMemory == null
                        ? "n/a"
                        : string.Format("{0:n0} Mb", HugeCrossInfo.TotalMemory / 1024)
                          + (memAvailable.HasValue && memFree.HasValue ? $" ({(memAvailable.Value / 1024):n0} available, {(memFree.Value/1024):n0} free)" : ""),
                };
                
                var broadcastMessage = new
                {
                    MessageId = MessageId,
                    NewVer = NewVersionDataSource.NewVersion,
                    // Hostname = Environment.MachineName,
                    System = hostInfo,
                    Block = blockStatView,
                    Mounts = mounts,
                    Net = netStatView,
                    // Disk = new Dictionary<string,object>()
                };
                
#if DEBUG
                Console.WriteLine($"Background broadcast took {sw}");
#endif
                
                hubContext.Clients.All.SendAsync("ReceiveDataSource", broadcastMessage);
                DebugDumper.Dump(broadcastMessage, "BroadcastMessage.json");
                DebugDumper.Dump(broadcastMessage, "BroadcastMessage.min.json", minify: true);
            }

#if DUMPS
            // Console.WriteLine($"DataSource flushed: {sw.Elapsed}");
#endif
        }
    }
}
