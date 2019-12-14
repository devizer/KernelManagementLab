using System.Diagnostics;

namespace Universe.Dashboard.Agent
{
    class TempStopwatch
    {
        private Stopwatch _stopwatch;
        private CpuUsage.CpuUsage _cpuUsage;

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
                _cpuUsage = CpuUsage.CpuUsage.GetByThread() ?? new CpuUsage.CpuUsage();
            }
        }

        public override string ToString()
        {
            if (IsDebug)
            {
                var secs = _stopwatch.ElapsedTicks / (double) Stopwatch.Frequency;
                var next = CpuUsage.CpuUsage.GetByThread() ?? new CpuUsage.CpuUsage();
                var delta = CpuUsage.CpuUsage.Substruct(next, _cpuUsage);

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