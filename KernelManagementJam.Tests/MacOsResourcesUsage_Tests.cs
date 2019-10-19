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
        [Test]
        public void mach_thread_self_Test1()
        {
            int threadId = MacOsThreadInfoInterop.mach_thread_self();
            Console.WriteLine($"Thread ID: {threadId}");
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
        
    }
}