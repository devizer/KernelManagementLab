using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using KernelManagementJam.ThreadInfo;
using NUnit.Framework;

namespace Tests
{
    public class NUnitTestsBase
    {
        public static bool IsTravis => Environment.GetEnvironmentVariable("TRAVIS") == "true";

        protected static TextWriter OUT;
        private Stopwatch StartAt;
        private LinuxResources? _LinuxResources_OnStart;
        private int TestCounter = 0;

        [SetUp]
        public void BaseSetUp()
        {
            TestConsole.Setup();
            Environment.SetEnvironmentVariable("SKIP_FLUSHING", null);
            StartAt = Stopwatch.StartNew();
            _LinuxResources_OnStart = GetLinuxResources();
            Interlocked.Increment(ref TestCounter);
            Console.WriteLine($"#{TestCounter} {{{TestContext.CurrentContext.Test.Name}}} starting...");
        }

        LinuxResources? GetLinuxResources()
        {
            if (LinuxResourceUsage.IsSupported)
            {
                try
                {
                    return LinuxResourceUsage.GetByThread();
                }
                catch
                {
                }
            }

            return null;
        }

        [TearDown]
        public void BaseTearDown()
        {
            string cpuUsage = "";
            if (_LinuxResources_OnStart.HasValue)
            {
                var onEnd = GetLinuxResources();
                if (onEnd != null)
                {
                    var delta = LinuxResources.Substruct(onEnd.Value, _LinuxResources_OnStart.Value);
                    var user = delta.UserUsage.TotalMicroSeconds / 1000d;
                    var kernel = delta.KernelUsage.TotalMicroSeconds / 1000d;
                    cpuUsage = $" (user: {user:n3}, kernel: {kernel:n3} milliseconds)";
                }
            }
            Console.WriteLine($"#{TestCounter} {{{TestContext.CurrentContext.Test.Name}}} >{TestContext.CurrentContext.Result.Outcome.Status.ToString().ToUpper()}< in {StartAt.Elapsed}{cpuUsage}{Environment.NewLine}");
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
//                    TestContext.Progress.Write(string.Join(",", value.Select(x => ((int)x).ToString("X2"))) );
//                    if (value.Length > Environment.NewLine.Length && value.EndsWith(Environment.NewLine))
//                        value = value.Substring(0, value.Length - Environment.NewLine.Length);
                    
                    
                    TestContext.Progress.WriteLine(value);
                    try
                    {
                        // TestContext.Error.WriteLine(value); // .WriteLine();
                    }
                    catch
                    {
                    }
                }

            }
        }

    }
}