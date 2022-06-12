using System.Linq;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;

namespace Universe.Dashboard.Agent
{
    public class CpuTemperatureTimer
    {
        public static void Process()
        {
            var baseReportKey = new AdvancedMiniProfilerKeyPath(SharedDefinitions.RootKernelMetricsObserverKey, "CpuTempInfo::Timer");
            PreciseTimer.AddListener("CpuTempInfo::Timer", () =>
            {
                using (AdvancedMiniProfiler.Step(baseReportKey))
                {
                    var sensors = LinuxHwmonParser.GetAll();
                    CpuTemperatureDataSource.Instance = sensors.ToList();
                }
            });
        }
        
    }
}
