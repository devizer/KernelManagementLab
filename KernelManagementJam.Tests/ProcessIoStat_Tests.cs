using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Tests;

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
        
        [Test]
        public void Test_UserName()
        {
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.UserName == Environment.UserName));
        }

        [Test]
        public void Test_ParentPid()
        {
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.ParentPid > 0));
        }

        [Test]
        public void Test_FullAccess()
        {
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => !x.IsAccessDenied));
        }

        [Test]
        public void Test_KernelCpuUsage()
        {
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.KernelCpuUsage > 0));
        }

        [Test]
        public void Test_UserCpuUsage()
        {
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.UserCpuUsage > 0));
        }

        [Test]
        public void Test_IoTime()
        {
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.IoTime > 0));
        }

        [Test]
        public void Test_ReadBytes()
        {
            var processes = ProcessIoStat.GetProcesses();
            Assert.IsTrue(processes.Any(x => x.ReadBytes > 0), "At least one process has ReadBytes");
            Assert.IsTrue(processes.Where(x => x.IsAccessDenied).All(x => x.ReadBytes == 0), "All the processes without access has empty ReadBytes");
        }
        
        [Test]
        public void Express_Benchmark()
        {
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
            
            Console.WriteLine($"Processes: {nProcs}, Benchmark: {(1000d*n/(double)msecs):n2}");
        }

    }
}