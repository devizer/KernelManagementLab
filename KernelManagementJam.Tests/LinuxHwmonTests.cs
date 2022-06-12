using System;
using System.Diagnostics;
using KernelManagementJam.DebugUtils;
using NUnit.Framework;
using Universe;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    
    public class LinuxHwmonTests : NUnitTestsBase
    {
        [Test]
        [TestCase("1st")]
        [TestCase("2nd")]
        public void Perform(string counter)
        {
            if (CrossInfo.ThePlatform != CrossInfo.Platform.Linux) return;
            
            Stopwatch sw = Stopwatch.StartNew();
            var sensors = LinuxHwmonParser.GetAll();
            var milliseconds = sw.ElapsedTicks * 1000d / (double) Stopwatch.Frequency;
            Console.WriteLine($"HWMON Sensors:{Environment.NewLine}{sensors.AsJson()}{Environment.NewLine}Took {milliseconds:f3} milliseconds");
        }
    }
}
