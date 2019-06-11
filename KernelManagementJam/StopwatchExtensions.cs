using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;

namespace KernelManagementJam
{
    public static class StopwatchLog
    {
        class Consolas : IDisposable
        {
            internal string Caption = null;
            internal Stopwatch Stowatch = null;
            private static long Counter = 0;
            private long Id;

            public Consolas()
            {
                Id = Interlocked.Increment(ref Counter);
            }

            public void Dispose()
            {
                var msec = Stowatch.ElapsedTicks * 1000d / Stopwatch.Frequency;
                Console.WriteLine($"Stopwatch #{Id}: {Caption} in {msec:n2} msec");
            }
        }
        
        public static IDisposable ToConsole(string caption)
        {
            return new Consolas
            {
                Caption = caption, 
                Stowatch = Stopwatch.StartNew()
            };
        }
    }
}