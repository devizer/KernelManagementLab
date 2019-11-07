using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Resources;
using System.Text;

namespace KernelManagementJam.ThreadInfo
{
    public class AdvancedStopwatch
    {
        public static readonly long Frequency;
        public static readonly bool IsHighResolution;

        private CpuUsage startedCpuUsage;
        private Stopwatch startedDuration;
        private bool isRunning = false;

        private CpuUsage elapsedCpuUsage;

        public bool IsRunning
        {
            get { return isRunning; }
        }

        static AdvancedStopwatch()
        {
            Frequency = Stopwatch.Frequency;
            IsHighResolution = Stopwatch.IsHighResolution;
        }

        public AdvancedStopwatch()
        {
            Reset();
        }

        public void Reset()
        {
            elapsedCpuUsage = new CpuUsage();
            elapsedTicks = 0;
            isRunning = false;
            startedDuration.Reset();
            startedCpuUsage = new CpuUsage();
        }

        public void Restart()
        {
            startedDuration.Restart();
            elapsedTicks = 0;
            elapsedCpuUsage = new CpuUsage();
            startedCpuUsage = CpuUsageReader.GetByThread() ?? new CpuUsage();
            isRunning = true;
        }

        public TimeSpan Elapsed
        {
            get { return startedDuration.Elapsed; }
        }



    }
}
