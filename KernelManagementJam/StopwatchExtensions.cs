using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;
using KernelManagementJam.ThreadInfo;

namespace KernelManagementJam
{
    public static class StopwatchLog
    {
        class Consolas : IDisposable
        {
            internal string Caption = null;
            internal Stopwatch Stowatch = null;
            internal CpuUsage? CpuUsageAtStart = null;
            
            private static long Counter = 0;
            private long Id;

            public Consolas()
            {
                Id = Interlocked.Increment(ref Counter);
            }

            public void Dispose()
            {
                double msec = Stowatch.ElapsedTicks * 1000d / Stopwatch.Frequency;
                string cpuUsage = "";
                if (CpuUsageAtStart.HasValue)
                {
                    var onEnd = GetCpuUsage();
                    if (onEnd != null)
                    {
                        var delta = CpuUsage.Substruct(onEnd.Value, CpuUsageAtStart.Value);
                        // milli seconds
                        double user = delta.UserUsage.TotalMicroSeconds / 1000d;
                        double kernel = delta.KernelUsage.TotalMicroSeconds / 1000d;
                        double perCents = (user + kernel) / msec; 
                        cpuUsage = $" (cpu: {(perCents*100):f0}%, {(user+kernel):n3} = {user:n3} [user] + {kernel:n3} [kernel] milliseconds)";
                    }
                }

                Console.WriteLine($"Stopwatch #{Id}: {Caption} in {msec:n2} msec {cpuUsage}");
            }
            
            internal static CpuUsage? GetCpuUsage()
            {
                try
                {
                    // return LinuxResourceUsage.GetByThread();
                    return CpuUsageReader.Get(CpuUsageScope.Thread);
                }
                catch
                {
                }

                return null;
            }

        }
        
        public static IDisposable ToConsole(string caption)
        {
            return new Consolas
            {
                Caption = caption, 
                Stowatch = Stopwatch.StartNew(),
                CpuUsageAtStart = Consolas.GetCpuUsage(),
            };
        }
    }
}