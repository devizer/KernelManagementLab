using System;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;

namespace Universe.Dashboard.Agent
{
    public class CpuFreqInfoSummaryTimer
    {
        public static void Process()
        {
            var baseReportKey = new AdvancedMiniProfilerKeyPath(SharedDefinitions.RootKernelMetricsObserverKey, "CpuFreqInfo::Timer");
            PreciseTimer.AddListener("CpuFreqInfo::Timer", () =>
            {
                using (AdvancedMiniProfiler.Step(baseReportKey))
                {
                    var cpus = CpuFreqInfoParser.Parse();
                    CpuFreqDataSource.Instance = cpus;
                }
            });
        }
        
    }
}