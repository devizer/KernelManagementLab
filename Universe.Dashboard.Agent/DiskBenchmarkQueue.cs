using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KernelManagementJam;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.Agent
{
    // Threadsafe, deterministic order
    public class DiskBenchmarkQueue
    {
        public class DiskBenchmarkWithToken
        {
            public Guid Token;
            public IDiskBenchmark Benchmark;
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

        public void Enqueue(Guid token, IDiskBenchmark benchmark)
        {
            var item = new DiskBenchmarkWithToken() {Token = token, Benchmark = benchmark};
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

                found.Benchmark.RequestCancel();
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
                };
            }
        }

        public class EnlistedDiskBenchmark
        {
            // 0: InProgress, >0: Pending, <0: not found 
            public int Index;
            public Guid Token;
            public IDiskBenchmark Benchmark;
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
                            // Console.WriteLine($"Disk Benchmark Job failed. {ex.GetExceptionDigest()}{Environment.NewLine}{ex}");
                        }

                        if (!benchmark.IsCanceled)
                        {
                            var entity = new DiskBenchmarkEntity()
                            {
                                Args = benchmark.Parameters,
                                Token = nextJob.Token.ToString(),
                                Report = benchmark.Progress, // Progress is complete
                                CreatedAt = DateTime.UtcNow,
                                MountPath = benchmark.Parameters.WorkFolder,
                                ErrorInfo = benchmarkException?.GetExceptionDigest(),
                            };

                            using (var db = GetDbContext())
                            {
                                db.DiskBenchmark.Add(entity);
                                db.SaveChanges();
                            }
                        }
                    }
                    finally
                    {
                        lock (SyncQueue) Queue.Remove(nextJob);
                    }
                }

                WaitHandle.WaitAll(new[] {(WaitHandle)Waiter, PreciseTimer.Shutdown}, 499);
            }
        }
    }
}