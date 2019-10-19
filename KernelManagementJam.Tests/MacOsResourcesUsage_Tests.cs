using System;
using System.Runtime.InteropServices;
using KernelManagementJam.ThreadInfo;
using NUnit.Framework;
using Tests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class MacOsResourcesUsage_Tests : NUnitTestsBase
    {
        private static bool IsMacOs => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        
        [Test]
        public void _1_mach_thread_self_Test1()
        {
            if (!IsMacOs) return;
            
            int threadId = MacOsThreadInfoInterop.mach_thread_self();
            Console.WriteLine($"Thread ID: {threadId}");
            int resDe = MacOsThreadInfoInterop.mach_port_deallocate(MacOsThreadInfoInterop.mach_thread_self(), threadId);
            Console.WriteLine($"mach_port_deallocate result: {resDe}");
        }

        [Test]
        public void _2_thread_info_Test1()
        {
            if (!IsMacOs) return;

            int threadId = MacOsThreadInfoInterop.mach_thread_self();
            Console.WriteLine($"Thread ID: {threadId}");

            var raw = MacOsThreadInfoInterop.GetRawThreadInfo(threadId);
            for(int i=0; i<raw.Length; i++)
                Console.WriteLine($"  {i}: {raw[i]}");
            
            int resDe = MacOsThreadInfoInterop.mach_port_deallocate(MacOsThreadInfoInterop.mach_thread_self(), threadId);
            Console.WriteLine($"mach_port_deallocate result: {resDe}");
        }

        [Test]
        public void _3_thread_info_Test2_Custom()
        {
            if (!IsMacOs) return;

            int threadId = MacOsThreadInfoInterop.mach_thread_self();
            Console.WriteLine($"Thread ID: {threadId}");

            var raw = MacOsThreadInfoInterop.GetRawThreadInfo(threadId);
            for(int i=0; i<raw.Length; i++)
                Console.WriteLine($"  {i}: {raw[i]}");
            
            int resDe = MacOsThreadInfoInterop.mach_port_deallocate(MacOsThreadInfoInterop.mach_thread_self(), threadId);
            Console.WriteLine($"mach_port_deallocate result: {resDe}");
        }
    }

    class MacOsThreadInfoInterop
    {
        [DllImport("libc", SetLastError = false, EntryPoint = "mach_thread_self")]
        public static extern int mach_thread_self();
        
        // mach_port_deallocate
        [DllImport("libc", SetLastError = false, EntryPoint = "mach_port_deallocate")]
        public static extern int mach_port_deallocate(int threadId, int materializedThreadId);
        
        [DllImport("libc", SetLastError = false)]
        public static extern int thread_info(int threadId, int flavor, ref ThreadInfo info, int count);
        

        public class ThreadInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 10)]
            public int[] Raw;
        }

        public static int[] GetRawThreadInfo(int threadId)
        {
            ThreadInfo info = new ThreadInfo() {Raw = new int[10]};
            int count = 40;
            int result = thread_info(threadId, 3, ref info, count);
            Console.WriteLine($"thread_info return value:${result}");
            return info.Raw;
        }

        [DllImport("libc", SetLastError = false, EntryPoint = "thread_info")]
        public static extern int thread_info_custom(int threadId, int flavor, IntPtr threadInfo, int count);
        public static unsafe int[] GetRawThreadInfo_Custom(int threadId)
        {
            IntPtr threadInfo = Marshal.AllocHGlobal(40);
            int count = 40;
            int result = thread_info_custom(threadId, 3, threadInfo, count);
            Console.WriteLine($"thread_info return value:${result}");

            int[] ret = new int[10];
            int* ptr = (int*) threadInfo.ToPointer();
            for (int i = 0; i < 10; i++)
            {
                ret[i] = *ptr;
                ptr++;
            }
            
            Marshal.FreeHGlobal(threadInfo);
            return ret;
        }

        
    }
}