using System;
using System.Diagnostics;

namespace Universe.Benchmark.DiskBench
{
    class CpuUsageInProgress
    {
        private CpuUsage.CpuUsage? CpuUsageOnStart;
        private Stopwatch Stopwatch;
        long PrevElapsed;

        public CpuUsage.CpuUsage? Result;

        public static CpuUsageInProgress StartNew()
        {
            var cpuUsageHelper = new CpuUsageInProgress();
            cpuUsageHelper.Stopwatch = Stopwatch.StartNew();
            cpuUsageHelper.Restart();
            return cpuUsageHelper;
        }

        public void Restart()
        {
            CpuUsageOnStart = CpuUsage.CpuUsage.GetByThread();
            Stopwatch.Restart();
            PrevElapsed = 0;
        }

        // returns true if progress updated
        public bool AggregateCpuUsage(bool force = false)
        {
            var nextElapsed = Stopwatch.ElapsedMilliseconds;
            if (nextElapsed > PrevElapsed + 333 || force)
            {
                PrevElapsed = nextElapsed;
                if (CpuUsageOnStart.HasValue)
                {
                    CpuUsage.CpuUsage? cpuUsageNext = CpuUsage.CpuUsage.GetByThread();
                    if (cpuUsageNext.HasValue)
                    {
                        Result = CpuUsage.CpuUsage.Substruct(cpuUsageNext.Value, CpuUsageOnStart.Value);
                        // Console.WriteLine($"AggregateCpuUsage(force:{force}): {Result}");
                        return true;
                    }
                }
            }

            return false;
        }

    }
}