using System;
using NUnit.Framework;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class LinuxMemorySummary_Tests
    {

        [Test]
        public void Does_Work()
        {
            if (!LinuxMemorySummary.TryParse(out var info))
                Assert.Fail("LinuxMemorySummary.TryParse is not supported");
            
            Console.WriteLine($"LinuxMemorySummary: {info}");

        }
    }
}