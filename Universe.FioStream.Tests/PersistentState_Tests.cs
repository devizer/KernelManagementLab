using NUnit.Framework;
using Tests;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class PersistentState_Tests : NUnitTestsBase
    {
        [Test]
        public void Test_Strings()
        {
            var string1 = PersistentState.GetOrStore("key1-A/key2-B", () => "SS777");
            Assert.AreEqual("SS777", string1, "string1 is SS777");
            var string2 = PersistentState.GetOrStore("key1-A/key2-B", () => "Never Evaluated");
            Assert.AreEqual("SS777", string2, "string2 is SS777");
        }

        [Test]
        [TestCase(false, TestName = "Bool=False")]
        [TestCase(true, TestName = "Bool=True")]
        public void Test_Strings(bool expected)
        {
            bool bool1 = PersistentState.GetOrStore($"test-bool/try-{expected}", () => expected);
            Assert.AreEqual(expected, bool1, $"bool1 is {expected}");
            bool bool2 = PersistentState.GetOrStore($"test-bool/try-{expected}", () => !expected);
            Assert.AreEqual(expected, bool1, $"bool1 is {expected}");
        }
        
    }
}