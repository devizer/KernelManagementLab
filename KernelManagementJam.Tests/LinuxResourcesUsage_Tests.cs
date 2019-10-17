using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KernelManagementJam.ThreadInfo;
using NUnit.Framework;
using Tests;

namespace KernelManagementJam.Tests
{
    // git pull; time dotnet test --filter LinuxResourcesUsage
    [TestFixture]
    public class LinuxResourcesUsage_Tests : NUnitTestsBase
    {
        [Test]
        public void Is_Supported()
        {
            Console.WriteLine($"LinuxResourceUsage.IsSupported: {LinuxResourceUsage.IsSupported}");
        }

        [Test]
        public void Get__Raw_Thread_Usage()
        {
            var resources = LinuxResourceUsageInterop.GetRawUsageResources(LinuxResourceUsageInterop.RUSAGE_THREAD);
            Console.WriteLine($"GetRawUsageResources(RUSAGE_THREAD):{Environment.NewLine}{AsString(resources)}");
        }

        [Test]
        public void Get__Raw_Process_Usage()
        {
            var resources = LinuxResourceUsageInterop.GetRawUsageResources(LinuxResourceUsageInterop.RUSAGE_SELF);
            Console.WriteLine($"GetRawUsageResources(RUSAGE_SELF):{Environment.NewLine}{AsString(resources)}");
        }

        [Test]
        public void Get_Process_Usage()
        {
            var usage = LinuxResourceUsage.GetByProcess();
            Console.WriteLine($"LinuxResourceUsage.GetByProcess(): {usage}");
        }

        [Test]
        public void Get_Thread_Usage()
        {
            var usage = LinuxResourceUsage.GetByThread();
            Console.WriteLine($"LinuxResourceUsage.GetByThread(): {usage}");
        }

        static string AsString(IEnumerable arr)
        {
            var count = arr.OfType<object>().Count();
            StringBuilder b = new StringBuilder();
            int n = 0;
            foreach (var v in arr)
            {
                var comma = n + 1 < count ? "," : "";
                b.AppendFormat("{0,-14}", $"{n}:{v}{comma}");
                n++;
                b.Append(" ");
                if (n % 4 == 0) b.Append(Environment.NewLine);
            }

            return b.ToString();
        }
        
    }
}