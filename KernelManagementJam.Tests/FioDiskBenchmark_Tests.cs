using System;
using System.Collections.Generic;
using System.Linq;
using KernelManagementJam.Benchmarks;
using KernelManagementJam.DebugUtils;
using NUnit.Framework;
using Universe.Benchmark.DiskBench;
using Universe.FioStream;
using Universe.FioStream.Binaries;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class FioDiskBenchmark_Tests : NUnitTestsBase
    {
        private FioEnginesProvider _EnginesProvider;

        public class PicoLogger : IPicoLogger
        {
            public static PicoLogger Instance = new PicoLogger();
            public void LogInfo(string message) => Console.WriteLine(message);
            public void LogWarning(string message) => Console.WriteLine($"WARNING! {message}");
        }

        [Test]
        public void _1_Discovery()
        {
            var logger = new PicoLogger();
            var featuresCache = new FioFeaturesCache() {Logger = logger};
            _EnginesProvider = new FioEnginesProvider(featuresCache, logger);
            Assert.IsTrue(_EnginesProvider.GetEngines().Count == 0, "engines.Count == 0 on start");
            _EnginesProvider.Discovery();

            List<FioEnginesProvider.Engine> engines = _EnginesProvider.GetEngines();
            Assert.IsTrue(engines.Count > 0, "engines.Count > 0 after");
        }
        
        [Test]
        public void _2_Run()
        {
            if (_EnginesProvider == null) { _1_Discovery(); }
            
            FioEnginesProvider.Engine engine = _EnginesProvider?.GetEngines().FirstOrDefault();
            Console.WriteLine($"First discovered engine: [{engine}]");
            Assert.NotNull(engine);
            var options = new DiskBenchmarkOptions()
            {
                StepDuration = 1000,
                ThreadsNumber = 2,
                WorkFolder = Environment.CurrentDirectory,
                WorkingSetSize = 1024 * 1024,
                RandomAccessBlockSize = 512,
            };
            
            FioDiskBenchmark fio = new FioDiskBenchmark(options)
            {
                Engine = engine
            };
            
            fio.Perform();
            Console.WriteLine(fio.Progress.AsJson());
        }
        
    }
}
