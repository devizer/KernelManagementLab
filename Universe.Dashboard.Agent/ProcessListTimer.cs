using System;
using System.Diagnostics;
using KernelManagementJam;

namespace Universe.Dashboard.Agent
{
    public class ProcessListTimer
    {
        private const int SLOW_PC_UPDATE_INTERVAL = 8000;
        private const int FAST_PC_UPDATE_INTERVAL = 4000;

        public static void Process()
        {
            var intervalMilliseconds = InternalMilliseconds.Value;
            Console.WriteLine($"Process List Update INTERVAL is {intervalMilliseconds}");
            ProcessListDataSource.ProcessListUpdateInterval = intervalMilliseconds;
            
            var prev = ProcessIoStat.GetProcesses();

            var skipOn = (intervalMilliseconds + 999) / 1000;
            var iteration = 0;
            PreciseTimer.AddListener("Processes::Timer", () =>
            {
                if (iteration == 0)
                {
                    var next = ProcessIoStat.GetProcesses();
                }

                iteration = (iteration + 1) % skipOn;
            });
        }

        private static Lazy<int> InternalMilliseconds = new Lazy<int>(() =>
        {
            int ret = SLOW_PC_UPDATE_INTERVAL;
            const string EnvKey = "PROCESS_LIST_UPFATE_INTERVAL";
            var raw = Environment.GetEnvironmentVariable(EnvKey);
            if (raw != null && int.TryParse(raw, out var interval))
            {
                ret = Math.Max(1000, Math.Min(30000, interval));
            }
            else
            {
                var benchmark = GetBenchmark();
                // ivy bridge 3.7 GHz is 80000
                if (benchmark >= 40000) ret = FAST_PC_UPDATE_INTERVAL; // interval 1 sec for core 2 duo and above
            }

            return ret;
        });

        private static unsafe int GetBenchmark()
        {
            // if performance is good then 1000 msec
            Stopwatch sw = Stopwatch.StartNew();
            int size = 1024, benchmark = 0;
            int* buffer = stackalloc int[size];
            while (sw.ElapsedMilliseconds < 100)
            {
                for (int i = 0; i < size; i++) buffer[i] = buffer[(i + 2) % size] + 1;
                benchmark++;
            }
#if DEBUG || true
            Console.WriteLine($"Synthetic BENCHMARK: {benchmark} (ivy bridge 3.7 GHz is 80000");
#endif
            return benchmark;
        }
    }
    
    
}