using NUnit.Framework;
using Tests;
using Universe;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class UptimeParser_Tests : NUnitTestsBase
    {

        [Test]
        public void Does_Work()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Linux)
                Assert.Greater(UptimeParser.ParseUptime().Value, 0.00001);
        }
    }
}