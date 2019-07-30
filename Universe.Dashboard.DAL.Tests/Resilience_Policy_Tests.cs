using System;
using NUnit.Framework;
using Polly;
using Universe.Dashboard.DAL;

namespace Tests
{
    public class Resilience_Policy_Tests : NUnitTestsBase
    {
        [Test]
        public void Write_v1()
        {
            Policy p = DbResilience.WritingPolicy;
            p.Execute(context => throw new Exception("No Way"), new Context("Save DiskBenchmark History"));
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