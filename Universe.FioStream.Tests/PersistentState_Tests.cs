using System.Linq;
using NUnit.Framework;
using Universe.NUnitTests;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class PersistentState_Tests : NUnitTestsBase
    {
        [Test]
        public void Test_Strings()
        {
            var string1 = PersistentState.GetOrStore("test-string/key-Z", () => "42^^42");
            Assert.AreEqual("42^^42", string1, "string1 is 42^^42");
            var string2 = PersistentState.GetOrStore("test-string/key-Z", () => "Never Evaluated");
            Assert.AreEqual("42^^42", string2, "string2 is 42^^42");
        }

        [Test]
        [TestCase(false, TestName = "Bool=False")]
        [TestCase(true, TestName = "Bool=True")]
        public void Test_Bool(bool expected)
        {
            bool bool1 = PersistentState.GetOrStore($"test-bool/try-{expected}", () => expected);
            Assert.AreEqual(expected, bool1, $"bool1 is {expected}");
            bool bool2 = PersistentState.GetOrStore($"test-bool/try-{expected}", () => !expected);
            Assert.AreEqual(expected, bool1, $"bool1 is {expected}");
        }

        [Test]
        [TestCase(3,new string[] {"one", "two", "three"}, TestName = "Array of 3 strings")]
        [TestCase(0,new string[0], TestName = "Empty array")]
        public void Test_Array_of_Strings(long info, string[] expected)
        {
            string[] arr1 = PersistentState.GetOrStore($"test-string[]/try-{info}", () => expected);
            CollectionAssert.AreEqual(expected, arr1, $"arr1 is ok");
            string[] arr2 = PersistentState.GetOrStore($"test-string[]/try-{info}", () => new[] {"never", "evaluated"});
            CollectionAssert.AreEqual(expected, arr2, $"arr2 is ok");
        }
        
    }
}