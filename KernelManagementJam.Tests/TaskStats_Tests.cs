using System.Diagnostics;
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
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Linux)
                Assert.AreEqual(Process.GetCurrentProcess().Id, TaskStatInterop.get_pid());
            
        }
    }
}