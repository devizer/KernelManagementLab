using System;
using KernelManagementJam;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;

namespace Universe.W3Top
{
    class DbPreJitter
    {

        public static void PreJIT()
        {

            using (StopwatchLog.ToConsole("PreJIT DB's Metrics History logic"))
            {
                using (DashboardContext db = new DashboardContext())
                {
                    HistoryLogic history = new HistoryLogic(db);
                    history.Save("StartAt", new {At = DateTime.UtcNow});
                }
            }

            using (StopwatchLog.ToConsole("PreJIT DB's Disk Benchmark History logic")) 
            using(DashboardContext db = new DashboardContext())
            {
                
                DiskBenchmarkDataAccess benchmarkDA = new DiskBenchmarkDataAccess(db);
                DiskBenchmark benchmark = new DiskBenchmark(".");

                Guid token = Guid.NewGuid();
                var entity = new DiskBenchmarkEntity()
                {
                    Args = benchmark.Parameters,
                    Token = token,
                    Report = benchmark.Progress, // Progress is complete
                    CreatedAt = DateTime.UtcNow,
                    MountPath = benchmark.Parameters.WorkFolder,
                    ErrorInfo = "incomplete",
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
                db.SaveChanges();
            }
            
        }
    }
}