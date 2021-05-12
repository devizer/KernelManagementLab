using System;
using NUnit.Framework;
using Tests;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class Features_Tests : NUnitTestsBase
    {
        private FioFeaturesCache FeaturesCache = new FioFeaturesCache() {Logger = new PicoLogger()};

        [Test]
        public void Test_for_Current_Platform()
        {
            var candidates = Candidates.GetCandidates();
            Console.WriteLine($"Checking [{candidates.Count}] candidates for [{Candidates.PosixSystem}] running on [{Candidates.PosixMachine}] cpu");
            foreach (var bin in candidates)
            {
                var features = FeaturesCache[bin];
                var engines = features.EngineList;
                var enginesInfo = engines == null ? "null" : string.Join(",", engines);
                Console.WriteLine($"{bin.Name}: [{features.Version}] [{enginesInfo}]");
            }

        }
        
    }
}