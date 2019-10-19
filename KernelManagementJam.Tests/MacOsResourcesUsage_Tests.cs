using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        
        [Test, Ignore("doesnt work")]
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

            var raw = MacOsThreadInfoInterop.GetRawThreadInfo_Custom(threadId);
            var maxLen = raw.Select(x => x.ToString().Length).Max();
            Console.WriteLine("Thread Info: " + string.Join(" | ", raw.Select((x,i) => string.Format("{0,2}:{1,-" + maxLen + "}", i, x))));
//            for(int i=0; i<raw.Length; i++)
//                Console.Write($"  {i}: {raw[i]}");
            
            int resDe = MacOsThreadInfoInterop.mach_port_deallocate(MacOsThreadInfoInterop.mach_thread_self(), threadId);
            Console.WriteLine($"mach_port_deallocate result: {resDe}");
        }

        [Test]
        public void _4_thread_info_Test3_Custom()
        {
            int interval = 11;
            for (int i = 0; i < 5; i++)
            {
                _3_thread_info_Test2_Custom();
                LoadThread(interval);
                interval = Math.Min(interval * 2, 1111);
            }

        }
        
        static void LoadThread(long milliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < milliseconds)
                new Random().Next();
            
        }

        
    }

    class MacOsThreadInfoInterop
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
        public static int[] GetRawThreadInfo(int threadId)
        {
            ThreadInfo info = new ThreadInfo() {Raw = new int[10]};
            int count = 40;
            int result = thread_info(threadId, 3, ref info, ref count);
            Console.WriteLine($"thread_info return value:${result}");
            return info.Raw;
        }

        [DllImport("libc", SetLastError = true, EntryPoint = "thread_info")]
        public static extern int thread_info_custom(int threadId, int flavor, IntPtr threadInfo, ref int count);
        public static unsafe int[] GetRawThreadInfo_Custom(int threadId)
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