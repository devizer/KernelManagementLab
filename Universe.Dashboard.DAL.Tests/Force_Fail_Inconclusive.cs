using NUnit.Framework;
using Universe.NUnitTests;

namespace Tests
{
    public class Force_Fail_Inconclusive : NUnitTestsBase
    {
        [Test/*, Explicit*/]
        public void Forced_Fail()
        {
            Assert.Fail("Forced Fail");
        }
    }
}