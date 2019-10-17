using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace KernelManagementJam.ThreadInfo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LinuxResources
    {
        public LinuxTime UserUsage;
        public LinuxTime KernelUsage;

        public override string ToString()
        {
            return $"User: {UserUsage}, Kernel: {KernelUsage}";
        }
    }

    [StructLayout(LayoutKind.Sequential)] 
    public struct LinuxTime
    {
        public long Seconds;
        public long MicroSeconds;

        public long TotalMicroSeconds => Seconds * 1000000 + MicroSeconds;
        public double TotalSeconds => Seconds + MicroSeconds / 1000000d;


        public override string ToString()
        {
            return $"{TotalMicroSeconds / 1000d:n2} milliseconds";
        }
    }

    public enum LinuxResourcesScope
    {
        Thread,
        Process,
    }

    public class LinuxResourceUsage
    {

        public static bool IsSupported => _IsSupported.Value;
        public static LinuxResources? GetByScope(LinuxResourcesScope scope)
        {
            var s = scope == LinuxResourcesScope.Process ? LinuxResourceUsageInterop.RUSAGE_SELF : LinuxResourceUsageInterop.RUSAGE_THREAD; 
            return GetLinuxResourcesByScope(s);
        }
        
        public static LinuxResources? GetByProcess()
        {
            return GetLinuxResourcesByScope(LinuxResourceUsageInterop.RUSAGE_SELF);
        }

        public static LinuxResources? GetByThread()
        {
            return GetLinuxResourcesByScope(LinuxResourceUsageInterop.RUSAGE_THREAD);
        }

        static Lazy<bool> _IsSupported = new Lazy<bool>(() =>
        {
            try
            {
                GetByScope(LinuxResourcesScope.Process);
                GetByScope(LinuxResourcesScope.Thread);
                return true;
            }
            catch
            {
                return false;
            }
        });

        private static LinuxResources? GetLinuxResourcesByScope(int scope)
        {
            if (IntPtr.Size == 4)
            {
                RawLinuxResourceUsage_32 ret = new RawLinuxResourceUsage_32();
                ret.Raw = new int[18];
                int result = LinuxResourceUsageInterop.getrusage32(scope, ref ret);
                if (result != 0) return null;
                return new LinuxResources()
                {
                    UserUsage = new LinuxTime() {Seconds = ret.Raw[0], MicroSeconds = ret.Raw[1]},
                    KernelUsage = new LinuxTime() {Seconds = ret.Raw[2], MicroSeconds = ret.Raw[3]},
                };
            }
            else
            {
                RawLinuxResourceUsage_64 ret = new RawLinuxResourceUsage_64();
                ret.Raw = new long[18];
                int result = LinuxResourceUsageInterop.getrusage64(scope, ref ret);
                if (result != 0) return null;
                return new LinuxResources()
                {
                    UserUsage = new LinuxTime() {Seconds = ret.Raw[0], MicroSeconds = ret.Raw[1]},
                    KernelUsage = new LinuxTime() {Seconds = ret.Raw[2], MicroSeconds = ret.Raw[3]},
                };
            }
        }
    }
    
    public class LinuxResourceUsageInterop
    {

        // For integration tests
        public static IList GetRawUsageResources(int scope = RUSAGE_THREAD)
        {
            if (IntPtr.Size == 4)
            {
                RawLinuxResourceUsage_32 ret = new RawLinuxResourceUsage_32();
                ret.Raw = new int[18];
                int result = getrusage32(scope, ref ret);
                if (result != 0) return null;
                Console.WriteLine($"getrusage returns {result}");
                return ret.Raw;
            }
            else
            {
                RawLinuxResourceUsage_64 ret = new RawLinuxResourceUsage_64();
                ret.Raw = new long[18];
                int result = getrusage64(scope, ref ret);
                if (result != 0) return null;
                Console.WriteLine($"getrusage returns {result}");
                return ret.Raw;
            }
        }

        public const int RUSAGE_SELF = 0;
        public const int RUSAGE_CHILDREN = -1;
        public const int RUSAGE_BOTH = -2;         /* sys_wait4() uses this */
        public const int RUSAGE_THREAD = 1;        /* only the calling thread */
        
        
        [DllImport("libc", SetLastError = true, EntryPoint = "getrusage")]
        public static extern int getrusage32(int who, ref RawLinuxResourceUsage_32 resourceUsage);

        [DllImport("libc", SetLastError = true, EntryPoint = "getrusage")]
        public static extern int getrusage64(int who, ref RawLinuxResourceUsage_64 resourceUsage);
    }

    // https://github.com/mono/mono/issues?utf8=%E2%9C%93&q=getrusage
    // https://elixir.bootlin.com/linux/v2.6.24/source/include/linux/time.h#L19
    // https://stackoverflow.com/questions/1468443/per-thread-cpu-statistics-in-linux
    // ! http://man7.org/linux/man-pages/man2/getrusage.2.html
    // 

    
    [StructLayout(LayoutKind.Sequential)] 
    public struct RawLinuxResourceUsage_64
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 18)]
        public long[] Raw;
    }

    [StructLayout(LayoutKind.Sequential)] 
    public struct RawLinuxResourceUsage_32
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 18)]
        public int[] Raw;
    }
}