using System;
using System.Linq;
using NUnit.Framework;
using Tests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class CpuFreqInfo_Tests : NUnitTestsBase
    {
        [Test]
        public void Test()
        {
            var cpus = CpuFreqInfoParser.Parse();
            Console.WriteLine(string.Join(Environment.NewLine, cpus.Select((x, i) => $"{i}: {x}")));
            var asHtml = cpus.ToShortHtmlInfo();
            Console.WriteLine($"HTML: '{asHtml}'");
        }
    }
}