using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    public class NUnitTestsBase
    {
        public static bool IsTravis => Environment.GetEnvironmentVariable("TRAVIS") == "true";

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
        
        
        public class TestConsole
        {
            static bool Done = false;
            public static void Setup()
            {
                if (!Done)
                {
                    Done = true;
                    Console.SetOut(new TW());
                }
            
            }

            class TW : TextWriter
            {
                public override Encoding Encoding { get; }

                public override void WriteLine(string value)
                {
                    TestContext.Progress.WriteLine(value);
                }

                public override void Write(string format, params object[] arg)
                {
                    this.Write(string.Format(format, arg));
                }
            }
        }

    }
}