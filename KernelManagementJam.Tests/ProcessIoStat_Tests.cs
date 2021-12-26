using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Universe.NUnitTests;
using Universe;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class ProcessIoStat_Tests /*: NUnitTestsBase*/
    {

        [Test]
        public void Does_Work()
        {
            var processes = ProcessIoStat.GetProcesses();
            Assert.Greater(processes.Length, 0);
            foreach (var process in processes)
            {
                Console.WriteLine(process);
            }
        }

        private bool IsSupported
        {
            get
            {
                var ret = CrossInfo.ThePlatform == CrossInfo.Platform.Linux;
                if (!ret) Console.WriteLine($"The platform [{CrossInfo.ThePlatform}] is not supported for ProcessIoStat tests");
                return ret;
            }
        }

        [Test]
        public void Test_UserName()
        {
            if (!IsSupported) return;
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.UserName == Environment.UserName));
        }

        [Test]
        public void Test_ParentPid()
        {
            if (!IsSupported) return;
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.ParentPid > 0));
        }

        [Test]
        public void Test_FullAccess()
        {
            if (!IsSupported) return;
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => !x.IsAccessDenied));
        }

        [Test]
        public void Test_KernelCpuUsage()
        {
            if (!IsSupported) return;
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.KernelCpuUsage > 0));
        }

        [Test]
        public void Test_UserCpuUsage()
        {
            if (!IsSupported) return;
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.UserCpuUsage > 0));
        }

        [Test]
        public void Test_IoTime()
        {
            if (!IsSupported) return;
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.IoTime > 0));
        }

        [Test]
        public void Test_ReadBytes()
        {
            if (!IsSupported) return;
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.ReadBytes > 0), "At least one process has ReadBytes");
            Assert.IsTrue(processes.Where(x => x.IsAccessDenied).All(x => x.ReadBytes == 0), "All the processes without access has empty ReadBytes");
        }
        
        [Test]
        public void Express_Benchmark()
        {
            if (!IsSupported) return;
            ProcessIoStat.GetProcesses();
            Stopwatch sw = Stopwatch.StartNew();
            int n = 0;
            int nProcs;
            long msecs;
            do
            {
                n++;
                var processes = ProcessIoStat.GetProcesses();
                nProcs = processes.Length;
                msecs = sw.ElapsedMilliseconds;
            } while (msecs <= 1000);

            double msecPerAllProcesses = (1000d*n/(double)msecs);
            double msecPerProcess = 1000000d / msecPerAllProcesses / nProcs;
            Console.WriteLine($"Processes: {nProcs}, Benchmark: {msecPerAllProcesses:n2} rounds per seconds (a round is all the processes)");
            Console.WriteLine($"Benchmark per 1 process: {(msecPerProcess):n2} microseconds");
        }

    }
}