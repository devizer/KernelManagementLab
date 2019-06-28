using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using Universe.Dashboard.DAL;

namespace Tests
{
    public class NUnitTestsBase
    {
        protected static TextWriter OUT;
        private Stopwatch StartAt;
        private int TestCounter = 0;

        [SetUp]
        public void BaseSetUp()
        {
            Environment.SetEnvironmentVariable("SKIP_FLUSHING", null);
            StartAt = Stopwatch.StartNew();
            Interlocked.Increment(ref TestCounter);
            Console.WriteLine($"#{TestCounter} {{{TestContext.CurrentContext.Test.Name}}} starting...");
        }

        [TearDown]
        public void BaseTearDown()
        {
            Console.WriteLine($"#{TestCounter} {{{TestContext.CurrentContext.Test.Name}}} >{TestContext.CurrentContext.Result.Outcome.Status.ToString().ToUpper()}< in {StartAt.Elapsed}{Environment.NewLine}");
        }

        [OneTimeSetUp]
        public void BaseOneTimeSetUp()
        {
            TestConsole.Setup();
        }

        [OneTimeTearDown]
        public void BaseOneTimeTearDown()
        {
        }
        
        protected static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

    }
}