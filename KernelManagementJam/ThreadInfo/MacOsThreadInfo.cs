using System;
using System.Runtime.InteropServices;
using KernelManagementJam.ThreadInfo;

namespace KernelManagementJam.Tests
{
    public class MacOsThreadInfo
    {
        public static bool IsSupported => _IsSupported.Value;
        public static CpuUsage? GetByThread()
        {
            return Get();
        }

        static CpuUsage? Get()
        {
            int threadId = MacOsThreadInfoInterop.mach_thread_self();
            try
            {
                if (threadId == 0) return null;

                var raw = MacOsThreadInfoInterop.GetRawThreadInfo(threadId);
                if (raw != null) return new CpuUsage()
                {
                    UserUsage = new TimeValue() {Seconds = raw[0], MicroSeconds = raw[1]},
                    KernelUsage = new TimeValue() {Seconds = raw[2], MicroSeconds = raw[3]},
                };
            }
            finally
            {
                int resDeallocate = MacOsThreadInfoInterop.mach_port_deallocate(MacOsThreadInfoInterop.mach_thread_self(), threadId);
            }

            return null;
        }

        private static Lazy<bool> _IsSupported = new Lazy<bool>(() =>
        {
            try
            {
                GetByThread();
                return true;
            }
            catch
            {
                return false;
            }
        });

    }
    
    public class MacOsThreadInfoInterop
    {
        [DllImport("libc", SetLastError = false, EntryPoint = "mach_thread_self")]
        public static extern int mach_thread_self();
        
        // mach_port_deallocate
        [DllImport("libc", SetLastError = false, EntryPoint = "mach_port_deallocate")]
        public static extern int mach_port_deallocate(int threadId, int materializedThreadId);
        
        [DllImport("libc", SetLastError = true)]
        public static extern int thread_info(int threadId, int flavor, ref ThreadInfo info, ref int count);
        

        public class ThreadInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 10)]
            public int[] Raw;
        }

        // Doesnt work:
        // 
        public static int[] GetRawThreadInfo_Malformed(int threadId)
        {
            ThreadInfo info = new ThreadInfo() {Raw = new int[10]};
            int count = 40;
            int result = thread_info(threadId, 3, ref info, ref count);
            Console.WriteLine($"thread_info return value:${result}");
            return info.Raw;
        }

        [DllImport("libc", SetLastError = true, EntryPoint = "thread_info")]
        public static extern int thread_info_custom(int threadId, int flavor, IntPtr threadInfo, ref int count);
        
        public static unsafe int[] GetRawThreadInfo(int threadId)
        {
            int[] raw = new int[10];
            fixed (int* ptr = &raw[0])
            {
                int count = 40;
                IntPtr threadInfo = new IntPtr(ptr);
                int result = thread_info_custom(threadId, 3, threadInfo, ref count);
                if (result != 0) return null;
                return raw;
            }
        }

        public static unsafe int[] GetRawThreadInfo_Custom(int threadId)
        {
            int[] raw = new int[10];
            fixed (int* ptr = &raw[0])
            {
                int count = 40;
                IntPtr threadInfo = new IntPtr(ptr);
                int result = thread_info_custom(threadId, 3, threadInfo, ref count);
                Console.WriteLine($"thread_info return value: {result}");
                return raw;
            }
        }
        
        public static unsafe int[] GetRawThreadInfo_Custom_Legacy(int threadId)
        {
            IntPtr threadInfo = Marshal.AllocHGlobal(40);
            try
            {
                int count = 40;
                int result = thread_info_custom(threadId, 3, threadInfo, ref count);
                Console.WriteLine($"thread_info return value: {result}");

                int[] ret = new int[10];
                int* ptr = (int*) threadInfo.ToPointer();
                for (int i = 0; i < 10; i++)
                {
                    ret[i] = *ptr;
                    ptr++;
                }
                
                return ret;
            }
            finally
            {
                Marshal.FreeHGlobal(threadInfo);
            }
        }
        
    }
}