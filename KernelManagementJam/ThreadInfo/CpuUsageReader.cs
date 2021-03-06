using System;
using System.Runtime.InteropServices;
using KernelManagementJam.Tests;
using Universe;

namespace KernelManagementJam.ThreadInfo
{
    public class CpuUsageReader
    {
        public static TempCpuUsage? GetByProcess()
        {
            return Get(CpuUsageScope.Process);
        }

        public static TempCpuUsage? GetByThread()
        {
            return Get(CpuUsageScope.Thread);
        }

        public static TempCpuUsage? SafeGet(CpuUsageScope scope)
        {
            try
            {
                return Get(scope);
            }
            catch
            {
                return null;
            }
        }
        
        public static TempCpuUsage? Get(CpuUsageScope scope)
        {
            if (scope == CpuUsageScope.Process)
            {
                if (IsLinux() || IsMacOs())
                    return LinuxResourceUsage.GetByProcess();
                else
                    // throw new NotSupportedException("CPU Usage in the scope of the process is supported on Linux and OS X only");
                    return WindowsCpuUsage.Get(CpuUsageScope.Process);
            }

            if (IsLinux())
                return LinuxResourceUsage.GetByThread();
            
            else if (IsMacOs())
                return MacOsThreadInfo.GetByThread();
            
            else if (IsWindows())
                // throw new NotImplementedException("CPU Usage in the scope of the thread is not yet implemented for Windows");
                return WindowsCpuUsage.Get(CpuUsageScope.Thread);
            
            throw new InvalidOperationException($"CPU usage in the scope of {scope} is a kind of an unknown on the {HugeCrossInfo.ThePlatform}");
        }

        static bool IsWindows()
        {
#if NETCORE || NETSTANDARD
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
            return HugeCrossInfo.ThePlatform == HugeCrossInfo.Platform.Windows;
#endif
        }
        static bool IsLinux()
        {
#if NETCORE || NETSTANDARD
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#else
            return HugeCrossInfo.ThePlatform == HugeCrossInfo.Platform.Linux;
#endif
        }
        static bool IsMacOs()
        {
#if NETCORE || NETSTANDARD
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#else
            return HugeCrossInfo.ThePlatform == HugeCrossInfo.Platform.MacOSX;
#endif
        }
    }
}