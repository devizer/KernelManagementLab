using System;
using KernelManagementJam.DebugUtils;
using NUnit.Framework;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class AdvancedMiniProfilerTests : NUnitTestsBase
    {
        [Test]
        public void _1_Simple()
        {
            for (var iteration = 1; iteration <= 2; iteration++)
            {
                var key = new AdvancedMiniProfilerKeyPath("Tests", "First Sub Test");
                var subKey = key.Child("Sub Sub Child");
                using (AdvancedMiniProfiler.Step(subKey))
                {
                    CrossPlatformCpuUsage_Tests.LoadThread(100);
                }
            }

            Console.WriteLine("As Table" + Environment.NewLine + AdvancedMiniProfilerReport.Instance.AsConsoleTable());
            Console.WriteLine("As Tree Table" + Environment.NewLine + AdvancedMiniProfilerReport.Instance.AsTreeTable());

            var reportCopy = AdvancedMiniProfilerReport.Instance.AsPlainCopy;
            var reportItems = reportCopy.Count;
            Assert.Greater(reportItems, 0);
        }
    }
}
