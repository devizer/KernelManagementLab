using System;
using System.Runtime.InteropServices;

namespace KernelManagementJam.ThreadInfo
{
    public class ThreadUsage
    {

        public static RawLinuxResourceUsage GetRawThreadResources()
        {
            RawLinuxResourceUsage ret = new RawLinuxResourceUsage();
            ret.Raw = new int[IntPtr.Size == 4 ? 18 : 36];
            int result = getrusage(RUSAGE_THREAD, ref ret);
            Console.WriteLine($"getrusage returns {result}");
            return ret;
        }

        private const int RUSAGE_SELF = 0;
        private const int RUSAGE_CHILDREN = -1;
        private const int RUSAGE_BOTH = -2;         /* sys_wait4() uses this */
        private const int RUSAGE_THREAD = 1;        /* only the calling thread */
        
        
        [DllImport("libc", SetLastError = true)]
        public static extern int getrusage(int who, ref RawLinuxResourceUsage resourceUsage);

        
    }

    // https://github.com/mono/mono/issues?utf8=%E2%9C%93&q=getrusage
    // https://elixir.bootlin.com/linux/v2.6.24/source/include/linux/time.h#L19
    // https://stackoverflow.com/questions/1468443/per-thread-cpu-statistics-in-linux
    // ! http://man7.org/linux/man-pages/man2/getrusage.2.html
    // 
    
    
    [StructLayout(LayoutKind.Sequential)] 
    public struct RawLinuxResourceUsage
    {
        // 36 for x64 and arm-64, 18 for arm-32
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 36)]
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