using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Universe;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class HugeCrossInfo_Tests : NUnitTestsBase
    {
        [Test]
        public void _1_ProcessorName()
        {
            Console.WriteLine($"Processor: [{HugeCrossInfo.ProcessorName}]");
            Assert.NotNull(HugeCrossInfo.ProcessorName);
            Assert.IsNotEmpty(HugeCrossInfo.ProcessorName.Trim());
        }

        [Test]
        public void _1a_ProcessorCores()
        {
            Console.WriteLine($"Platform: {CrossInfo.ThePlatform}");
            if (CrossInfo.ThePlatform != CrossInfo.Platform.Linux) return;

            var procCpuInfo = File.ReadAllText("/proc/cpuinfo");
            var procCores = HugeCrossInfo.GetLinuxProcCores(procCpuInfo);
            foreach (var processorCore in procCores)
            {
                Console.WriteLine(processorCore);
            }

            Console.WriteLine($"___________");
            Console.WriteLine($"KNOWN CORES: {procCores.Count}");

            Console.WriteLine($"CPU GROUPED NAME: {HugeCrossInfo.GetCpuPrettyName(procCores)}");
        }


        [Test]
        public void _2_OsName()
        {
            Console.WriteLine($"OS: [{HugeCrossInfo.OsDisplayName}]");
            Assert.NotNull(HugeCrossInfo.OsDisplayName);
            Assert.IsNotEmpty(HugeCrossInfo.OsDisplayName.Trim());
        }

        [Test]
        public void _3_MemorySize()
        {
            Console.WriteLine($"Total Memory: [{HugeCrossInfo.TotalMemory:n0}]");
            Assert.IsTrue(HugeCrossInfo.TotalMemory.GetValueOrDefault() > 0);
        }


        /*[Test]*/
        public void _4_Environment()
        {
            var allVars = Environment.GetEnvironmentVariables().Keys.OfType<string>().OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => $"{x}:{Environment.GetEnvironmentVariable(x)}")
                .ToList();

            Console.WriteLine($"{string.Join(Environment.NewLine, allVars)}");
        }


    }
}
