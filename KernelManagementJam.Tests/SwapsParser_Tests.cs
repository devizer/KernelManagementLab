using System;
using NUnit.Framework;
using Tests;
using Universe;
using Universe.CpuUsage;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class SwapsParser_Tests : NUnitTestsBase
    {
        [Test]
        public void Size_Is_Above_Zero()
        {
            if (CrossInfo.ThePlatform != CrossInfo.Platform.Linux) return;
            var swaps = SwapsParser.Parse();
            foreach (SwapInfo swapInfo in swaps)
            {
                Console.WriteLine($"Checking swap: {swapInfo}");
                Assert.Greater(swapInfo.Size, 0, $"A Swap Size of {swapInfo.Type} {swapInfo.FileName} should be above zero");
            }

        }
    }
}