using System;
using NUnit.Framework;
using Polly;
using Universe.Dashboard.DAL;

namespace Tests
{
    public class Resilience_Policy_Tests : NUnitTestsBase
    {
        [Test]
        public void Read_Fail()
        {
            decimal Do()
            {
                throw new Exception("Fail again");
                return 42;
            }
            
            decimal result = DbResilience.Query("Always fail", Do, 111, 222);
        }

        [Test]
        public void Read_Success()
        {
            long Do()
            {
                return 42;
            }
            
            long result = DbResilience.Query("Always 42", Do, 111, 222);
            Assert.AreEqual(42, result);
        }

        [Test]
        public void Write_v2()
        {
            DbResilience.ExecuteWriting(
                "My Save OP", 
                () => throw new Exception("No Way"),
                totalMilliseconds: 1000,
                retryCount: 9999999);
        }
    }
}