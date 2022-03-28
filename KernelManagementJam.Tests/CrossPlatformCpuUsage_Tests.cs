using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Universe.NUnitTests;
using Universe;
using Universe.CpuUsage;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class CrossPlatformCpuUsage_Tests : NUnitTestsBase
    {
        [Test]
        public void Grow_Usage_By_Thread()
        {
            GrowUsage_Impl(CpuUsageScope.Thread);
        }

        [Test]
        public void Grow_Usage_By_Process()
        {
            if (false && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Ignored on Windows");
                return;
            }
            
            GrowUsage_Impl(CpuUsageScope.Process);
        }

        private static void GrowUsage_Impl(CpuUsageScope scope)
        {
            if (false && HugeCrossInfo.ThePlatform == HugeCrossInfo.Platform.Windows && scope == CpuUsageScope.Process)
            {
                Console.WriteLine("Ignored on Windows");
                return;
            }
                
            
            LoadThread(1);
            Console.WriteLine($"Usage scope: {scope}");
            CpuUsage.Get(scope);
            var prev = CpuUsage.Get(scope);
            for (int i = 0; i < 10; i++)
            {
                LoadThread(9);
                CpuUsage? next = CpuUsage.Get(scope);
                Console.WriteLine($" {i} -> {next}");
                Assert.GreaterOrEqual(next.Value.KernelUsage.TotalMicroSeconds, prev.Value.KernelUsage.TotalMicroSeconds);
                Assert.GreaterOrEqual(next.Value.UserUsage.TotalMicroSeconds, prev.Value.UserUsage.TotalMicroSeconds);
                prev = next;
            }
        }

        [Test]
        public void Get_Process_Usage()
        {
            var usage = CpuUsage.GetByProcess();
            Console.WriteLine($"CpuUsage.GetByProcess(): {usage}");
        }
        

        [Test]
        public void Get_Thread_Usage()
        {
            var usage = CpuUsage.GetByThread();
            Console.WriteLine($"CpuUsage.GetByThread(): {usage}");
        }
        
        public static void LoadThread(long milliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < milliseconds)
                new Random().Next();
        }


    }
}
