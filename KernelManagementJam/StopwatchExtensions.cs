using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;
using Universe.CpuUsage;

namespace KernelManagementJam
{
    public static class StopwatchLog
    {
        internal static long Counter = 0; 
        class Consolas : IDisposable
        {
            internal string Caption = null;
            internal Stopwatch Stowatch = null;
            internal CpuUsage? CpuUsageAtStart = null;
            
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

                Console.WriteLine($"Stopwatch #{Id}: {Caption} in {msec:n3} msec{cpuUsage}");
            }
            
            internal static CpuUsage? GetCpuUsage()
            {
                try
                {
                    // return LinuxResourceUsage.GetByThread();
                    return CpuUsage.Get(CpuUsageScope.Thread);
                }
                catch
                {
                }

                return null;
            }
        }

        static Func<string> StartTimer()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            CpuUsage? atStart = CpuUsage.SafeGet(CpuUsageScope.Thread);
            
            return () =>
            {
                double msec = stopwatch.ElapsedTicks * 1000d / Stopwatch.Frequency;
                string cpuUsage = null;
                if (atStart.HasValue)
                {
                    var onEnd = CpuUsage.SafeGet(CpuUsageScope.Thread);
                    if (onEnd != null)
                    {
                        var delta = CpuUsage.Substruct(onEnd.Value, atStart.Value);
                        // milli seconds
                        double user = delta.UserUsage.TotalMicroSeconds / 1000d;
                        double kernel = delta.KernelUsage.TotalMicroSeconds / 1000d;
                        double perCents = (user + kernel) / msec; 
                        cpuUsage = $"{msec:n3} msec (cpu: {(perCents*100):f0}%, {(user+kernel):n3} = {user:n3} [user] + {kernel:n3} [kernel] milliseconds)";
                    }
                }

                if (cpuUsage == null)
                    cpuUsage = $"{msec:n3} msec";

                return cpuUsage;
            };
        }

        public static void SafeToConsole(string successCaption, string failWarning, Action action)
        {
            long id = Interlocked.Increment(ref Counter);
            var getMetrics = StartTimer();

            try
            {
                action();
                var cpuUsage = getMetrics();
                Console.WriteLine($"Stopwatch #{id}: {successCaption} in {cpuUsage}");
            }
            catch (Exception ex)
            {
                var cpuUsage = getMetrics();
                Console.WriteLine($"Warning! Stopwatch #{id} fail: {failWarning} in {cpuUsage}. " + ex.GetExceptionDigest() + Environment.NewLine + ex);
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
