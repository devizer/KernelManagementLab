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
    // git pull; dotnet test --filter ThreadResources
    [TestFixture]
    public class ThreadResources_Tests : NUnitTestsBase
    {
        [Test]
        public void SimpleGet()
        {
            var resources = ThreadUsage.GetRawThreadResources();
            var str = string.Join(",", resources);
            Console.WriteLine($"GetRawThreadResources():{Environment.NewLine}{AsString(resources)}");
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