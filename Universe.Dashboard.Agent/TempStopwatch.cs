using System.Diagnostics;

namespace Universe.Dashboard.Agent
{
    class TempStopwatch
    {
        private Stopwatch _stopwatch;
        private CpuUsage.CpuUsage _cpuUsageOnStart;

#if DEBUG        
        private const bool IsDebug = true;
#else        
        private const bool IsDebug = false;
#endif

        public TempStopwatch()
        {
            if (IsDebug)
            {
                _stopwatch = Stopwatch.StartNew();
                _cpuUsageOnStart = CpuUsage.CpuUsage.GetByThread().GetValueOrDefault();
            }
        }

        public override string ToString()
        {
            if (IsDebug)
            {
                double secs = _stopwatch.ElapsedTicks / (double) Stopwatch.Frequency;
                CpuUsage.CpuUsage next = CpuUsage.CpuUsage.GetByThread().GetValueOrDefault();
                CpuUsage.CpuUsage delta = CpuUsage.CpuUsage.Substruct(next, _cpuUsageOnStart);

                var perCents = secs > 0 ? $", {(delta.TotalMicroSeconds / 10000d / secs):n1}%" : "";

                return $"{secs * 1000:n3}{perCents} {delta}";
            }
            else
            {
                return "TempStopwatch is for DEBUG only";
            }
        }
    }
}