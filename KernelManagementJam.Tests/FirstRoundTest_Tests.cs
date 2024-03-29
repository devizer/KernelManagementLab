using NUnit.Framework;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class FirstRoundTest_Tests : NUnitTestsBase
    {
        [Test]
        public void FirstRoundTest_Simple_Tests()
        {
            int y1 = 0, y2 = 0, y3 = 0;
            FirstRound.RunOnce(() => y1 = 1, "test-1");
            FirstRound.RunOnce(() => y1 = 4242, "test-1");
            Assert.AreEqual(1, y1);

            
            FirstRound.RunOnly(() => y2 = 2, 1, "test-2");
            FirstRound.RunOnly(() => y2 = 4232, 1, "test-2");
            Assert.AreEqual(2, y2);
            
            FirstRound.RunTwice(() => y3 = 2, "test-3");
            FirstRound.RunTwice(() => y3 = 3, "test-3");
            FirstRound.RunTwice(() => y3 = 4343, "test-3");
            Assert.AreEqual(3, y3);
        }
    }
}
