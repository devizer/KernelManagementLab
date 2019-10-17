using System;
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
            var str = string.Join(",", resources.Raw);
            Console.WriteLine($"GetRawThreadResources(): {str}");
        }
        
    }
}