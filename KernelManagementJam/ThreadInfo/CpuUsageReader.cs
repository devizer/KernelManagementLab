using System;
using System.Runtime.InteropServices;
using KernelManagementJam.Tests;
using Universe;

namespace KernelManagementJam.ThreadInfo
{
    public class CpuUsageReader
    {
        public static CpuUsage? GetByProcess()
        {
            return Get(LinuxResourcesScope.Process);
        }

        // returns null on mac os x
        public static CpuUsage? GetByThread()
        {
            return Get(LinuxResourcesScope.Thread);
        }

        public static CpuUsage? Get(LinuxResourcesScope scope)
        {
            if (scope == LinuxResourcesScope.Process)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return LinuxResourceUsage.GetByProcess();
                else
                    throw new NotSupportedException("CPU Usage in the scope of the process is supported on Linux and OS X only");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return LinuxResourceUsage.GetByThread();
            
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return MacOsThreadInfo.GetByThread();
            
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                // throw new NotImplementedException("CPU Usage in the scope of the thread is not yet implemented for Windows");
                return WindowsCpuUsage.Get();
            
            throw new InvalidOperationException($"CPU usage in the scope of {scope} is a kind of an unknown on the {CrossInfo.ThePlatform}");
        }
    }
}