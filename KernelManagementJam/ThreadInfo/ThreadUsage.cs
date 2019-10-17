using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace KernelManagementJam.ThreadInfo
{
    public class ThreadUsage
    {

        public static IList GetRawThreadResources()
        {
            if (IntPtr.Size == 4)
            {
                RawLinuxResourceUsage_32 ret = new RawLinuxResourceUsage_32();
                ret.Raw = new int[18];
                int result = getrusage32(RUSAGE_THREAD, ref ret);
                Console.WriteLine($"getrusage returns {result}");
                return ret.Raw;
            }
            else
            {
                RawLinuxResourceUsage_64 ret = new RawLinuxResourceUsage_64();
                ret.Raw = new long[18];
                int result = getrusage64(RUSAGE_THREAD, ref ret);
                Console.WriteLine($"getrusage returns {result}");
                return ret.Raw;
            }
        }

        private const int RUSAGE_SELF = 0;
        private const int RUSAGE_CHILDREN = -1;
        private const int RUSAGE_BOTH = -2;         /* sys_wait4() uses this */
        private const int RUSAGE_THREAD = 1;        /* only the calling thread */
        
        
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
        // 36 for x64 and arm-64, 18 for arm-32
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 18)]
        public long[] Raw;
        // 1st - user time, 2nd is system time 
    }

    [StructLayout(LayoutKind.Sequential)] 
    public struct RawLinuxResourceUsage_32
    {
        // 36 for x64 and arm-64, 18 for arm-32
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 18)]
        public int[] Raw;
        // 1st - user time, 2nd is system time 
    }
    
    public struct TimeVal {
        public int	tv_sec;		/* seconds */
        public int	tv_usec;	/* microseconds */
    };
    
    public class ThreadStat
    {
        
    }
}