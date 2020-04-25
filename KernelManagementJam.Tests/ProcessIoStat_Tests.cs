using System;
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
    }
}