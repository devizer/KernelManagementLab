using System;
using System.Linq;
using KernelManagementJam.DebugUtils;
using NUnit.Framework;
using Universe;
using Universe.Benchmark.DiskBench;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class ReadonlyDiskBenchmark_Tests : NUnitTestsBase
    {
        [Test]
        public void Simple()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                return;
            
            var diskBenchmarkOptions = new DiskBenchmarkOptions()
            {
                StepDuration = 1,
                WorkFolder = Environment.CurrentDirectory,
                ThreadsNumber = 2,
                WorkingSetSize = 64*1024,
                RandomAccessBlockSize = 512,
            };

            Console.WriteLine($"ReadonlyDiskBenchmark for {diskBenchmarkOptions.WorkFolder}");
            ReadonlyDiskBenchmark ro = new ReadonlyDiskBenchmark(diskBenchmarkOptions);
            ro.Perform();
            Console.WriteLine(ro.Progress.AsJson());
            
            var progress = ro.Progress;
            string stepNames = string.Join(Environment.NewLine, progress.Steps.Select(x => $" --- {x.Name}: {x.AvgBytesPerSecond:n0}"));
            Console.WriteLine(stepNames);
            
            Assert.IsTrue(ro.Progress.IsCompleted);
        }
    }
}
