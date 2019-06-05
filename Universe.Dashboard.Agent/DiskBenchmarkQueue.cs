using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            public DiskBenchmark Benchmark;
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


        public void Enqueue(Guid token, DiskBenchmark benchmark)
        {
            var item = new DiskBenchmarkWithToken() {Token = token, Benchmark = benchmark};
            lock(SyncQueue) Queue.Add(item);
            Waiter.Set();
        }

        public DiskBenchmark Find(Guid token)
        {
            lock(SyncQueue) 
                return Queue.FirstOrDefault(x => x.Token == token)?.Benchmark;
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
                        var benchmark = nextJob.Benchmark;
                        benchmark.Perform();
                        using (var db = GetDbContext())
                        {
                            var entity = new DiskBenchmarkEntity()
                            {
                                Args = benchmark.Parameters,
                                Token = nextJob.Token.ToString(),
                                Report = benchmark.Prorgess, // Progress is complete
                                CreatedAt = DateTime.UtcNow,
                                MountPath = benchmark.Parameters.WorkFolder,
                            };
                            
                            db.DiskBenchmark.Add(entity);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Disk Benchmark Job failed. {ex.GetExceptionDigest()}{Environment.NewLine}{ex}");
                    }
                    finally
                    {
                        lock(SyncQueue) Queue.Remove(nextJob);
                    }
                }

                WaitHandle.WaitAll(new[] {(WaitHandle)Waiter, PreciseTimer.Shutdown}, 499);
            }
        }
    }
}