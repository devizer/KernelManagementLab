using System;
using System.Collections.Generic;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.Agent
{
    // Threadsafe, deterministic order
    public class DiskBenchmarkQueue
    {
        // private queue element
        private class DiskBenchmarkWithToken
        {
            public Guid Token;
            public IDiskBenchmark Benchmark;
            public DiskbenchmarkEnvironment Environment;
        }

        // for progress visualization
        public class EnlistedDiskBenchmark
        {
            // 0: InProgress, >0: Pending, <0: not found 
            public int Index;
            public Guid Token;
            public IDiskBenchmark Benchmark;
            public DiskbenchmarkEnvironment Environment;
        }
        
        private readonly Func<DashboardContext> GetDbContext;
        private List<DiskBenchmarkWithToken> Queue = new List<DiskBenchmarkWithToken>();
        private readonly object SyncQueue = new Object(); 
        AutoResetEvent Waiter = new AutoResetEvent(false);

        public DiskBenchmarkQueue(Func<DashboardContext> dbContext)
        {
            GetDbContext = dbContext;
            
            Thread t = new Thread(x => Process()) { Name = "Disk Benchmark Queue", IsBackground = true};
            t.Start();
        }

        public void Enqueue(Guid token, IDiskBenchmark benchmark, DiskbenchmarkEnvironment environment)
        {
            var item = new DiskBenchmarkWithToken() {Token = token, Benchmark = benchmark, Environment = environment};
            lock(SyncQueue) Queue.Add(item);
            Waiter.Set();
        }

        public bool Cancel(Guid token)
        {
            lock (SyncQueue)
            {
                var found = Find(token);
                if (found == null)
                    return false;

                if (found.Index > 0)
                {
                    Queue.RemoveAt(found.Index);
                    return true;
                }

                found.Benchmark?.RequestCancel();
                return true;
            }
        }

        public EnlistedDiskBenchmark Find(Guid token)
        {
            lock (SyncQueue)
            {
                int index = Queue.FindIndex(x => x.Token == token);
                if (index < 0)
                    return new EnlistedDiskBenchmark()
                    {
                        Token = token,
                        Index = -1,
                        Benchmark = null,
                    };

                return new EnlistedDiskBenchmark()
                {
                    Token = token,
                    Index = index,
                    Benchmark = Queue[index].Benchmark,
                    Environment = Queue[index].Environment,
                };
            }
        }


        public int Count
        {
            get
            {
                lock (SyncQueue) return Queue.Count;
            }
        }

        void Process()
        {
            while (!PreciseTimer.Shutdown.WaitOne(0))
            {
                DiskBenchmarkWithToken nextJob;
                lock(SyncQueue) nextJob = Queue.Count == 0 ? null : Queue[0];

                if (nextJob != null)
                {
                    try
                    {
                        Exception benchmarkException;
                        var benchmark = nextJob.Benchmark;
                        try
                        {
                            benchmark.Perform();
                            benchmarkException = null;
                        }
                        catch (Exception ex)
                        {
                            benchmarkException = ex;
                            Console.WriteLine($"Disk Benchmark Job failed. {ex.GetExceptionDigest()}{Environment.NewLine}{ex}");
                        }

                        if (!benchmark.IsCanceled)
                        {
                            var entity = new DiskBenchmarkEntity()
                            {
                                Args = benchmark.Parameters,
                                Token = nextJob.Token,
                                Report = benchmark.Progress, // Progress is complete
                                CreatedAt = DateTime.UtcNow,
                                MountPath = benchmark.Parameters.WorkFolder,
                                IsSuccess = benchmarkException == null,
                                ErrorInfo = benchmarkException?.GetExceptionDigest(),
                                Environment = nextJob.Environment,
                            };

                            using (var db = GetDbContext())
                            {
                                db.DiskBenchmark.Add(entity);

                                if (DebugDumper.AreDumpsEnabled)
                                {
                                    DebugDumper.Dump(entity.Report, "DiskBenchmark.Latest.Report.json", minify: false);
                                    DebugDumper.Dump(entity.Args, "DiskBenchmark.Latest.Args.json", minify: false);
                                    DebugDumper.Dump(entity.Environment, "DiskBenchmark.Latest.Environment.json", minify: false);
                                    DebugDumper.Dump(entity.ErrorInfo, "DiskBenchmark.Latest.ErrorInfo.json", minify: false);
                                    DebugDumper.Dump(entity, "DiskBenchmark.Latest.json", minify: false);
                                }
                                
                                // TODO: Process Crashes here
                                DbResilience.ExecuteWriting(
                                    "Save DiskBenchmark History", 
                                    () => db.SaveChanges(),
                                    totalMilliseconds: 15000,
                                    retryCount: 9999999);
                            }
                        }
                    }
                    finally
                    {
                        lock (SyncQueue) Queue.Remove(nextJob);
                    }
                }

                WaitHandle.WaitAny(new[] {(WaitHandle)Waiter, PreciseTimer.Shutdown}, 499);
            }
        }
    }
}