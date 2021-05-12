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

        private FioFeaturesCache FeaturesCache = Env.FeaturesCache;
        
        [Test]
        [TestCase(TestName = "A. Features First")]
        [TestCase(TestName = "B. Features Next")]
        public void Test_for_Current_Platform()
        {
            var candidates = Candidates.GetCandidates();
            Console.WriteLine(
                $"Checking [{candidates.Count}] candidates for [{Candidates.PosixSystem}] running on [{Candidates.PosixMachine}] cpu");
            List<string> okList = new List<string>();
            foreach (var bin in candidates)
            {
                var features = FeaturesCache[bin];
                var version = features.Version;
                var engines = features.EngineList;
                string enginesInfo = engines == null ? "null" : string.Join(",", engines);
                if (engines != null)
                {
                    List<string> enginesDetails = new List<string>();
                    foreach (var engine in engines)
                    {
                        bool isEngineSupported = features.IsEngineSupported(engine);
                        enginesDetails.Add($"{engine}{(isEngineSupported ? "+" : "-")}");
                    }

                    enginesInfo = string.Join(",", enginesDetails.ToArray());
                }

                var candidateInfo = $"{bin.Name}: [{version}] [{enginesInfo}]";
                Console.WriteLine(candidateInfo);
                if (engines != null && version != null)
                    okList.Add(candidateInfo);
            }

            var nl = Environment.NewLine;
            Console.WriteLine($"{nl}{nl}Found {okList.Count} candidates:{nl}{string.Join(nl, okList.ToArray())}");

        }

    }
}