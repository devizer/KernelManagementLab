using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class ProcessIoStat_Tests
    {

        [Test]
        public void Does_Work()
        {
            var processes = ProcessIoStat.GetProcesses();
            Assert.Greater(processes.Count, 0);
            foreach (var process in processes)
            {
                Console.WriteLine(process);
            }
        }
        
        [Test]
        public void Express_Bebchmark()
        {
            ProcessIoStat.GetProcesses();
            Stopwatch sw = Stopwatch.StartNew();
            int n = 0;
            int nProcs;
            long msecs;
            do
            {
                n++;
                List<ProcessIoStat> processes = ProcessIoStat.GetProcesses();
                nProcs = processes.Count;
                msecs = sw.ElapsedMilliseconds;
            } while (msecs <= 1000);
            
            Console.WriteLine($"Processes: {nProcs}, Benchmark: {(1000d*n/(double)msecs):n2}");
        }

    }
}