using System;
using System.Diagnostics;
using KernelManagementJam.ThreadInfo;
using NUnit.Framework;
using Tests;
using Universe;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class CrossPlatformCpuUsage_Tests : NUnitTestsBase
    {
        [Test]
        public void Grow_Usage_By_Thread()
        {
            GrowUsage_Impl(LinuxResourcesScope.Thread);
        }

        [Test]
        public void Grow_Usage_By_Process()
        {
            GrowUsage_Impl(LinuxResourcesScope.Process);
        }

        private static void GrowUsage_Impl(LinuxResourcesScope scope)
        {
            if (CrossInfo.ThePlatform != CrossInfo.Platform.MacOSX && CrossInfo.ThePlatform != CrossInfo.Platform.Linux)
                return;
            
            LoadThread(1);
            Console.WriteLine($"Usage scope: {scope}");
            LinuxResourceUsage.GetByScope(scope);
            var prev = LinuxResourceUsage.GetByScope(scope);
            for (int i = 0; i < 10; i++)
            {
                LoadThread(9);
                CpuUsage? next = LinuxResourceUsage.GetByScope(scope);
                Console.WriteLine($" {i} -> {next}");
                Assert.GreaterOrEqual(next.Value.KernelUsage.TotalMicroSeconds, prev.Value.KernelUsage.TotalMicroSeconds);
                Assert.GreaterOrEqual(next.Value.UserUsage.TotalMicroSeconds, prev.Value.UserUsage.TotalMicroSeconds);
                prev = next;
            }
        }

        [Test]
        public void Get_Process_Usage()
        {
            var usage = CpuUsageReader.GetByProcess();
            Console.WriteLine($"LinuxResourceUsage.GetByProcess(): {usage}");
        }
        

        [Test]
        public void Get_Thread_Usage()
        {
            var usage = LinuxResourceUsage.GetByThread();
            Console.WriteLine($"LinuxResourceUsage.GetByThread(): {usage}");
        }
        
        public static void LoadThread(long milliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < milliseconds)
                new Random().Next();
            
        }


    }
}