using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KernelManagementJam.ThreadInfo;
using NUnit.Framework;
using Tests;
using Universe;

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

            bool expectedSupported = CrossInfo.ThePlatform == CrossInfo.Platform.Linux ||
                                     CrossInfo.ThePlatform == CrossInfo.Platform.MacOSX;
            
            if (expectedSupported && !LinuxResourceUsage.IsSupported)
                Assert.Fail("On Linux 2.6.26+ the value of LinuxResourceUsage.IsSupported should be true");
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
        
        [Test]
        public void _Show_Raw_Thread_Usage()
        {
            var resources = LinuxResourceUsageInterop.GetRawUsageResources(LinuxResourceUsageInterop.RUSAGE_THREAD);
            Console.WriteLine($"GetRawUsageResources(RUSAGE_THREAD):{Environment.NewLine}{AsString(resources)}");
        }

        [Test]
        public void _Show_Raw_Process_Usage()
        {
            var resources = LinuxResourceUsageInterop.GetRawUsageResources(LinuxResourceUsageInterop.RUSAGE_SELF);
            Console.WriteLine($"GetRawUsageResources(RUSAGE_SELF):{Environment.NewLine}{AsString(resources)}");
        }


        
        static string AsString(IEnumerable arr)
        {
            if (arr == null) return "<null>";
            
            if (IntPtr.Size == 8 && CrossInfo.ThePlatform == CrossInfo.Platform.MacOSX)
            {
                var copy = arr.OfType<object>().ToArray();
                // microseconds on mac os are 4 bytes integers
                copy[1] = Convert.ToInt64(copy[1]) & 0xFFFFFFFF;
                copy[3] = Convert.ToInt64(copy[3]) & 0xFFFFFFFF;
                arr = copy;
            }
            
            const string rawNames = "u_sec u_usec k_sec k_usec maxrss ixrss idrss isrss minflt majflt nswap inblock oublock msgsnd msgrcv nsignals nvcsw nivcsw";
            var names = rawNames.Split(' ');
            
            var count = arr.OfType<object>().Count();
            StringBuilder b = new StringBuilder();
            int n = 0;
            foreach (var v in arr)
            {
                var comma = n + 1 < count ? "," : "";
                var name = n < names.Length ? (names[n] + " ") : "";
                name += n;
                name = name.PadLeft(11);
                b.AppendFormat("{0,-20}", $"{name}:{v}{comma}");
                n++;
                b.Append(" ");
                if (n % 4 == 0) b.Append(Environment.NewLine);
            }

            return b.ToString();
        }
        
    }
}