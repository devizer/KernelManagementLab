using System;
using System.Runtime.InteropServices;
using KernelManagementJam.ThreadInfo;

namespace KernelManagementJam.Tests
{
    public class WindowsCpuUsage
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadTimes(IntPtr hThread, out long lpCreationTime,
            out long lpExitTime, out long lpKernelTime, out long lpUserTime);

        public static bool GetThreadTimes(out long kernelMicroseconds, out long userMicroseconds)
        {
            long ignored;
            long kernel;
            long user;
            if (GetThreadTimes(GetCurrentThread(), out ignored, out ignored, out kernel, out user))
            {
                kernelMicroseconds = kernel * 100L;
                userMicroseconds = user * 100L;
                return true;
            }
            else
            {
                kernelMicroseconds = -1;
                userMicroseconds = -1;
                return false;
            }

        }

        public static CpuUsage? Get()
        {
            if (!GetThreadTimes(out long kernelMicroseconds, out long userMicroseconds))
                return null;

            const long m = 1000000L;
            return new CpuUsage()
            {
                KernelUsage = new TimeValue() { Seconds = kernelMicroseconds / m, MicroSeconds = kernelMicroseconds % m},
                UserUsage = new TimeValue() { Seconds = userMicroseconds / m, MicroSeconds = userMicroseconds % m},
            };

        }
    }
}