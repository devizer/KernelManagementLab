using System;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;

namespace Universe.Dashboard.Agent
{
    public class MemorySummaryTimer
    {
        public static void Process()
        {
            var baseReportKey = new AdvancedMiniProfilerKeyPath(SharedDefinitions.RootKernelMetricsObserverKey, "MemorySummary::Timer");
            PreciseTimer.AddListener("MemorySummary::Timer", () =>
            {
                using (AdvancedMiniProfiler.Step(baseReportKey))
                {
                    if (!LinuxMemorySummary.TryParse(out var info))
                        throw new NotSupportedException("LinuxMemorySummary.TryParse is not supported");
                    
                    var at = DateTime.UtcNow;
                    var logBy1Seconds = MemorySummaryDataSource.Instance.By_1_Seconds;
                    while (logBy1Seconds.Count >= 60 + 1)
                        logBy1Seconds.RemoveAt(0);

                    MemorySummaryDataSourcePoint point = new MemorySummaryDataSourcePoint()
                    {
                        At = at,
                        Summary = info,
                    };
                    
                    logBy1Seconds.Add(point);
                    if (DebugDumper.AreDumpsEnabled)
                    {
                        DebugDumper.Dump(logBy1Seconds, "MemorySummaryDataSourcePoint.1s.json");
                        DebugDumper.Dump(logBy1Seconds, "MemorySummaryDataSourcePoint.1s.min.json", minify: true);
                    }
                    
                }
            });
        }
    }
}