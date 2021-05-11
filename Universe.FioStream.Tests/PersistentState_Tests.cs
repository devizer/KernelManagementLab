using NUnit.Framework;
using Tests;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class PersistentState_Tests : NUnitTestsBase
    {
        [Test]
        public void Test1()
        {
            var string1 = PersistentState.GetOrStore("key1-A/key2-B", () => "SS777");
            Assert.AreEqual("SS777", string1, "string1 is SS777");
            var string2 = PersistentState.GetOrStore("key1-A/key2-B", () => "Never Evaluated");
            Assert.AreEqual("SS777", string2, "string2 is SS777");
        }
        
    }
}