using System;
using KernelManagementJam.DebugUtils;
using NUnit.Framework;
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
            var sensors = LinuxHwmonParser.GetAll();
            Console.WriteLine($"HWMON Sensors:{Environment.NewLine}{sensors.AsJson()}");
        }
    }
}
