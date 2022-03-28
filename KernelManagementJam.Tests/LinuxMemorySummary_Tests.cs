using System;
using NUnit.Framework;
using Universe;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class LinuxMemorySummary_Tests 
    {

        [Test]
        public void Does_Work()
        {
            if (CrossInfo.ThePlatform != CrossInfo.Platform.Linux) return;
            
            if (!LinuxMemorySummary.TryParse(out var info))
                Assert.Fail("LinuxMemorySummary.TryParse is not supported");
            
            Console.WriteLine($"LinuxMemorySummary: {info}");

        }
    }
}
