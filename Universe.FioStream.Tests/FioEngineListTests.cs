using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Tests;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class FioEngineListTests : NUnitTestsBase
    {
        [SetUp]
        public void SetUp() => FioStreamReader.ConsolasDebug = false;

        [Test]
        public void ShowEngineList()
        {
            var found = FindAllWorkingEngineList();
            Console.WriteLine($"{Environment.NewLine}{string.Join(Environment.NewLine,found)}");
        }
        
        static string[] FindAllWorkingEngineList()
        {
            List<string> ret = new List<string>();
            Stopwatch sw = Stopwatch.StartNew();
            var candidates = Candidates.GetCandidates();
            
            if (Env.ShortFioTests)
                candidates = candidates.Take(1).ToList();
            
            Console.WriteLine($"Checking [{candidates.Count}] candidates for [{Candidates.PosixSystem}] running on [{Candidates.PosixMachine}] cpu");
            foreach (var bin in candidates)
            {
                GZipCachedDownloader d = new GZipCachedDownloader();
                var cached = d.CacheGZip(bin.Name, bin.Url);
                var logger = new PicoLogger();
                FioChecker checker = new FioChecker(cached) { Logger = logger };
                var ver = checker.CheckVersion();
                if (ver != null)
                {
                    var summary = checker.CheckBenchmark(null, "--name=my --bs=1k --size=1k");
                    if (summary != null)
                    {
                        string[] engines;
                        try
                        {
                            FioEngineListReader rdr = new FioEngineListReader(cached);
                            engines = rdr.GetEngineList();
                            Console.WriteLine($"OK for [{cached}]: '{string.Join(", ", engines)}'");
                            ret.Add(cached);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning($"Can't obtain engine list for [{cached}]. {ex.Message}");
                        }
                    }
                }
            }

            Console.WriteLine($"Warning! All the candidates do not match, {sw.Elapsed}");
            return ret.ToArray();
        }
    }
}