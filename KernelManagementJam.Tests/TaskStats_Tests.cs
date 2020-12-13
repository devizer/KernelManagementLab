using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Universe;
using Universe.LinuxTaskStats;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class TaskStats_Tests
    {
        [Test]
        public void Test_Pid()
        {
            try
            {
                if (CrossInfo.ThePlatform == CrossInfo.Platform.Linux)
                    Assert.AreEqual(Process.GetCurrentProcess().Id, LinuxTaskStatsReader.GetPid());
            }
            catch (FileNotFoundException fnf)
            {
                Console.WriteLine("Warning! " + Environment.NewLine + fnf);
            }

        }
    }
}