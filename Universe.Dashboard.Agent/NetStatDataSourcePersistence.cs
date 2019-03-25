using System;
using System.Diagnostics;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.Agent
{
    public static class NetStatDataSourcePersistence
    {
        // x64: 338/67
        // Arm: 5200/518
        public static void Flush(DashboardContext db)
        {
            Stopwatch sw = Stopwatch.StartNew();
            HistoryLogic history = new HistoryLogic(db);
            // It is NOT thread safe
            history.Save("NetStatDataSource.By_1_Seconds", NetStatDataSource.Instance.By_1_Seconds);
            history.Save("NetStatDataSource", NetStatDataSource.Instance);
            double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
            Console.WriteLine($"History flushed in {msec:n1} milliseconds");
        }

        public static void PreJit()
        {
            DashboardContext db = new DashboardContext();
            HistoryLogic history = new HistoryLogic(db);
            history.Save("StartAt", new {At = DateTime.UtcNow});
        }
    }
}