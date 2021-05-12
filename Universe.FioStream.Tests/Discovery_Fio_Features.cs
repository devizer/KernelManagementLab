using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tests;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class Discovery_Fio_Features : NUnitTestsBase
    {
        private FioFeaturesCache FeaturesCache = Env.FeaturesCache;

        [Test]
        [TestCase(TestName = "A. Discovery First")]
        [TestCase(TestName = "B. Discovery Next")]
        public void Discover()
        {
            string[] linuxEngines = "io_uring,libaio,posixaio,rpvsync2,pvsync,vsync,psync,sync,mmap".Split(',');
            string[] windowsEngines = "windowsaio,posixaio,pvsync2,pvsync,vsync,psync,sync,mmap".Split(',');
            string[] osxEngines = "posixaio,pvsync2,pvsync,vsync,psync,sync,mmap".Split(',');
            string[] targetEngines;
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                targetEngines = windowsEngines;
            else if (CrossInfo.ThePlatform == CrossInfo.Platform.MacOSX)
                targetEngines = osxEngines;
            else
                targetEngines = linuxEngines;

            Dictionary<string, Candidates.Info> candidatesByEngines = new Dictionary<string, Candidates.Info>();

            var candidates = Candidates.GetCandidates();
            Console.WriteLine(
                $"Checking [{candidates.Count}] candidates for [{Candidates.PosixSystem}] running on [{Candidates.PosixMachine}] cpu");
            List<string> okList = new List<string>();
            foreach (var bin in candidates)
            {
                var features = FeaturesCache[bin];
                var version = features.Version;
                var engines = features.EngineList;
                if (engines == null) continue;
                if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                    if (!engines.Contains("windowsaio"))
                        engines = engines.Concat(new[] {"windowsaio"}).ToArray();

                var toFind = targetEngines
                    .Where(x => !candidatesByEngines.ContainsKey(x))
                    .Where(x => engines.Contains(x));

                foreach (var engine in toFind)
                {
                    Console.WriteLine($"Checking engine [{engine}] for [{bin.Name}]");
                    bool isEngineSupported = features.IsEngineSupported(engine);
                    if (isEngineSupported)
                    {
                        candidatesByEngines[engine] = bin;
                    }
                }
            }

            var nl = Environment.NewLine;
            var joined = string.Join(nl, candidatesByEngines.Select(x => $"{x.Key}: {x.Value.Name}").ToArray());
            Console.WriteLine($"{nl}{nl}Found {candidatesByEngines.Count} candidates: for engines{nl}{joined}");

        }
    }
}