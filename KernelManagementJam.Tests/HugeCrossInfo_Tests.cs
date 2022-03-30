using System;
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

        [Test]
        public void FirstRoundTest()
        {
            int y1 = 0, y2 = 0, y3 = 0;
            FirstRound.RunOnce(() => y1 = 1, "test-1");
            FirstRound.RunOnly(() => y2 = 2, 1, "test-2");
            FirstRound.RunTwice(() => y3 = 3, "test-3");
            Assert.AreEqual(1, y1);
            Assert.AreEqual(2, y2);
            Assert.AreEqual(3, y3);
        }
    }
}
