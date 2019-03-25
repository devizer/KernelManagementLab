using System;
using System.Diagnostics;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.Agent
{
    public static class NetStatDataSourcePersistence
    {
        public static void Flush()
        {
            Stopwatch sw = Stopwatch.StartNew();
            DashboardContext db = new DashboardContext();
            HistoryLogic history = new HistoryLogic(db);
            // It is NOT thread safe
            history.Save("NetStatDataSource.By_1_Seconds", NetStatDataSource.Instance.By_1_Seconds);
            history.Save("NetStatDataSource", NetStatDataSource.Instance);
            double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
            Console.WriteLine($"History flushed in {msec:n1} milliseconds");
        }
    }
}