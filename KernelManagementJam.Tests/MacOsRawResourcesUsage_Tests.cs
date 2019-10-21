using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Tests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class MacOsRawResourcesUsage_Tests : NUnitTestsBase
    {
        private static bool IsMacOs => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        
        public void _1_mach_thread_self_Test1()
        {
            if (!IsMacOs) return;
            
            int threadId = MacOsThreadInfoInterop.mach_thread_self();
            Console.WriteLine($"Thread ID: {threadId}");
            int resDe = MacOsThreadInfoInterop.mach_port_deallocate(MacOsThreadInfoInterop.mach_thread_self(), threadId);
            Console.WriteLine($"mach_port_deallocate result: {resDe}");
        }

        /*[Test, Ignore("doesnt work")]*/
        public void _2_thread_info_Test1()
        {
            if (!IsMacOs) return;

            int threadId = MacOsThreadInfoInterop.mach_thread_self();
            Console.WriteLine($"Thread ID: {threadId}");

            var raw = MacOsThreadInfoInterop.GetRawThreadInfo_Custom(threadId);
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
            
            int resDe = MacOsThreadInfoInterop.mach_port_deallocate(MacOsThreadInfoInterop.mach_thread_self(), threadId);
            Console.WriteLine($"mach_port_deallocate result: {resDe}");
        }

        [Test]
        public void _4_thread_info_Test3_Custom()
        {
            if (!IsMacOs) return;

            int interval = 11;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; sw.ElapsedMilliseconds < 6666; i++)
            {
                _3_thread_info_Test2_Custom();
                LoadThread(interval);
                interval = Math.Min(interval * 2, 7777);
            }
        }
        
        static void LoadThread(long milliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < milliseconds)
                new Random().Next();
        }

        
    }
}