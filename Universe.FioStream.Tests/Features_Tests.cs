using System;
using System.Collections.Generic;
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
        [TestCase(TestName = "A. Features First")]
        [TestCase(TestName = "B. Features Next")]
        public void Test_for_Current_Platform()
        {
            var candidates = Candidates.GetCandidates();
            Console.WriteLine($"Checking [{candidates.Count}] candidates for [{Candidates.PosixSystem}] running on [{Candidates.PosixMachine}] cpu");
            List<string> okList = new List<string>(); 
            foreach (var bin in candidates)
            {
                var features = FeaturesCache[bin];
                var engines = features.EngineList;
                var enginesInfo = engines == null ? "null" : string.Join(",", engines);
                var candidateInfo = $"{bin.Name}: [{features.Version}] [{enginesInfo}]";
                Console.WriteLine(candidateInfo);
                if (engines != null && features.Version != null)
                    okList.Add(candidateInfo);
            }

            var nl = Environment.NewLine;
            Console.WriteLine($"{nl}{nl}Found {okList.Count} candidates:{nl}{string.Join(nl,okList.ToArray())}");

        }
        
    }
}