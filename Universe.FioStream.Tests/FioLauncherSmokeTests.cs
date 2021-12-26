using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Universe.NUnitTests;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class FioLauncherSmokeTests : NUnitTestsBase
    {
        [SetUp]
        public void SetUp() => FioStreamReader.ConsolasDebug = false;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }


        string _DefaultFio = null;

        string DefaultFio
        {
            get
            {
                if (_DefaultFio == null) _DefaultFio = FindFirstWorkingCandidate() ?? "fio";
                return _DefaultFio;
            }
        }

        [Test]
        [TestCase(TestName = "A. First")]
        [TestCase(TestName = "B. Next")]
        public void GetTextVersion()
        {
            FioVersionReader verReader = new FioVersionReader(DefaultFio);
            var textVersion = verReader.GetTextVersion();
            Console.WriteLine($"fio version: [{textVersion}]");
        }

        [Test]
        public void GetVersion()
        {
            FioVersionReader verReader = new FioVersionReader(DefaultFio);
            var version = verReader.GetVersion();
            Console.WriteLine($"fio version: [{version}]");
        }

        [Test]
        [TestCase("--bs=4k --size=4k", TestName = "1. First")]
        [TestCase("--bs=4k --size=4k", TestName = "2. Next")]
        [TestCase("--time_based --bs=4k --size=128M --runtime=2 --ramp_time=2", TestName = "3. Big")]
        public void SmokeLaunch(string additionalArgs)
        {
            var args = $"--ioengine=sync --name=my --eta=always --filename=fiotest.tmp --iodepth=1 --readwrite=read {additionalArgs}";

            bool hasSummary = false;
            Action<StreamReader> handler = streamReader =>
            {
                FioStreamReader rdr = new FioStreamReader();
                rdr.NotifyEta += eta => Console.WriteLine($"ETA: [{eta}]");
                rdr.NotifyJobProgress += progress => Console.WriteLine($"PROGRESS: [{progress}]");
                rdr.NotifyJobSummary += summary =>
                {
                    Console.WriteLine($"SUMMARY: [{summary}]");
                    hasSummary = true;
                };
                rdr.ReadStreamToEnd(streamReader);
            };
            FioLauncher launcher = new FioLauncher(DefaultFio, args, handler);
            launcher.Start();
            if (!string.IsNullOrEmpty(launcher.ErrorText) || launcher.ExitCode != 0)
            {
                Console.WriteLine($"EXIT CODE: {launcher.ExitCode}");
                Console.WriteLine($"ERROR TEXT: {launcher.ErrorText}");
            }
            
            Assert.IsTrue(hasSummary, "Has Summary");
        }

        static string FindFirstWorkingCandidate()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var candidates = Candidates.GetCandidates();
            Console.WriteLine($"Checking [{candidates.Count}] candidates for [{Candidates.PosixSystem}] running on [{Candidates.PosixMachine}] cpu");
            foreach (var bin in candidates)
            {
                GZipCachedDownloader d = new GZipCachedDownloader();
                var cached = d.CacheGZip(bin.Name, bin.Url);
                FioChecker checker = new FioChecker(cached) {Logger = new PicoLogger()};
                var ver = checker.CheckVersion();
                if (ver != null)
                {
                    var summary = checker.CheckBenchmark(null, "--name=my --bs=1k --size=1k");
                    if (summary != null)
                    {
                        Console.WriteLine($"Selected: [{cached}] {ver}, {sw.Elapsed}");
                        return cached;
                    }
                }

            }

            Console.WriteLine($"Warning! All the candidates do not match, {sw.Elapsed}");
            return null;
        }
    }
}