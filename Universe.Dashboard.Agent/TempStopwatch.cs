using System.Diagnostics;

namespace Universe.Dashboard.Agent
{
    class TempStopwatch
    {
        private Stopwatch _stopwatch;
        private CpuUsage.CpuUsage _cpuUsage;

        public TempStopwatch()
        {
            _stopwatch = Stopwatch.StartNew();
            _cpuUsage = CpuUsage.CpuUsage.GetByThread() ?? new CpuUsage.CpuUsage();
        }

        public override string ToString()
        {
            var secs = _stopwatch.ElapsedTicks / (double) Stopwatch.Frequency;
            var next = CpuUsage.CpuUsage.GetByThread() ?? new CpuUsage.CpuUsage();
            var delta = CpuUsage.CpuUsage.Substruct(next, _cpuUsage);
            return $"{secs*1000:n3} ({delta}) milliseconds";
        }
    }
}