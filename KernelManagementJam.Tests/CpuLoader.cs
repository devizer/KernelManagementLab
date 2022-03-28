using System;
using System.Diagnostics;

namespace KernelManagementJam.Tests
{
    class CpuLoader
    {
        public static void LoadThread(long milliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < milliseconds)
                new Random().Next();
        }

    }
}
