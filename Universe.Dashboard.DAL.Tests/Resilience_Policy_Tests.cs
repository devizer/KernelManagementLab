using System;
using System.Threading;
using NUnit.Framework;
using Polly;
using Universe.Dashboard.DAL;

namespace Tests
{
    public class Resilience_Policy_Tests : NUnitTestsBase
    {
        [Test]
        public void Reading_Fails()
        {
            decimal Do()
            {
                Thread.Sleep(1);
                throw new Exception("Fail again");
            }

            var operationName = "Always fail";
            try
            {
                decimal result = DbResilience.Query(operationName, Do, 11, 22);
                Assert.Fail("TimeoutException expected");
            }
            catch (TimeoutException ex)
            {
                Assert.True(ex.Message.IndexOf(operationName) >= 0, "Exception message contains operation description");
            }
        }

        [Test]
        public void Successful_Read()
        {
            long Do()
            {
                return 42;
            }
            
            long result = DbResilience.Query("Always 42", Do, 111, 222);
            Assert.AreEqual(42, result);
        }

        [Test]
        public void Writing_Fails()
        {
            var operationName = "My Save OP";
            try
            {
                DbResilience.ExecuteWriting(
                    operationName,
                    () =>
                    {
                        Thread.Sleep(1);
                        throw new Exception("No Way");
                    },
                    totalMilliseconds: 11,
                    retryCount: 9999999);
                
                Assert.Fail("TimeoutException expected");
            }
            catch (TimeoutException ex)
            {
                Assert.True(ex.Message.IndexOf(operationName) >= 0, "Exception message contains operation description");
            }
        }
    }
}