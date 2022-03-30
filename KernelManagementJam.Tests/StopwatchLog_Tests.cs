using NUnit.Framework;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class StopwatchLog_Tests : NUnitTestsBase
    {
        [Test]
        public void StopwatchLog_Simple_Test()
        {
            using (StopwatchLog.ToConsole("Test StopwatchLog"))
            {
                CpuLoader.LoadThread(42);
            }
        }
    }
}
