using System;
using System.Diagnostics;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;

namespace ReactGraphLab
{
    class DbPreJitter
    {

        public static void PreJIT()
        {
            Stopwatch sw = Stopwatch.StartNew();
            DashboardContext db = new DashboardContext();
            {
                // Metrics history
                HistoryLogic history = new HistoryLogic(db);
                history.Save("StartAt", new {At = DateTime.UtcNow});
            }

            {
                // 
                DiskBenchmarkDataAccess benchmarkDA = new DiskBenchmarkDataAccess(db);
                DiskBenchmark benchmark = new DiskBenchmark(".");

                Guid token = Guid.NewGuid();
                var entity = new DiskBenchmarkEntity()
                {
                    Args = benchmark.Parameters,
                    Token = token.ToString(),
                    Report = benchmark.Prorgess, // Progress is complete
                    CreatedAt = DateTime.UtcNow,
                    MountPath = benchmark.Parameters.WorkFolder,
                    ErrorInfo = "in complete",
                };

                db.DiskBenchmark.Add(entity);
                db.SaveChanges();
                int idEntity = entity.Id;
                var copy1 = db.Find<DiskBenchmarkEntity>(idEntity);
                if (copy1 == null)
                    throw new Exception($"Can't retrieve DiskBenchmarkEntity by id = {idEntity}");

                var copy2 = benchmarkDA.GetDiskBenchmarkResult(token);
                if (copy2 == null)
                    throw new Exception($"Can't retrieve DiskBenchmarkEntity by token = {token}");

                db.DiskBenchmark.Remove(copy1);
            }
            
            Console.WriteLine($"DB Logic pre-jitted in {sw.Elapsed}");

        }
    }
}