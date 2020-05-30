using System.Diagnostics;
using NUnit.Framework;
using Universe.LinuxTaskstat;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class TaskStat_Tests
    {
        [Test]
        public void Test_Pid()
        {
            Assert.AreEqual(Process.GetCurrentProcess().Id, TaskStatInterop.get_pid());
        }
        
    }
}