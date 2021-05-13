using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Tests;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class BinariesSmokeTests : NUnitTestsBase
    {
        [SetUp]
        public void SetUp() => FioStreamReader.ConsolasDebug = false;

        [Test]
        public void Test_Linux_Candidates()
        {
            List<OrderedLinuxCandidates.LinuxCandidate> binaries = OrderedLinuxCandidates.AllLinuxCandidates;
                
            Console.WriteLine($"ALL Linux Candidates: [{binaries.Count}]");
            foreach (var bin in binaries)
            {
                Console.WriteLine(bin);
            }
        }

        private static string[] Architectures => "i386 armel armhf arm64 amd64 powerpc mips64el ppc64el".Split();

        [Test]
        // [TestCase(true, TestName = "1. No Cache (Linux)")]
        // [TestCase(false, TestName = "2. Allow (Linux)")]
        [TestCaseSource(typeof(BinariesSmokeTests), nameof(BinariesSmokeTests.Architectures))]
        public void Test_Download_Only_Linux(string arch)
        {
            GZipCachedDownloader.IgnoreCacheForDebug = true;
            Console.WriteLine(arch);
            var candidates = OrderedLinuxCandidates.AllLinuxCandidates
                .Where(x => arch.Equals(x.Arch, StringComparison.OrdinalIgnoreCase))
                .Select(x => new Candidates.Info()
                {
                    Name = x.Name,
                    Url = x.Url,
                }).ToArray();
            
            RunGetVersions(candidates);
        }

        [Test]
        [TestCase(true, TestName = "1. No Cache (Non Linux)")]
        [TestCase(false, TestName = "2. Allow Cache (Non Linux)")]
        public void Test_Download_Only_Non_Linux(bool ignoreCache)
        {
            GZipCachedDownloader.IgnoreCacheForDebug = ignoreCache;
            var binaries = Candidates.AllWindowsCandidates()
                .Concat(Candidates.AllMacOsCandidates())
                .ToArray();

            RunGetVersions(binaries);
        }

        private static void RunGetVersions(Candidates.Info[] binaries)
        {
            int n = 0;
            foreach (var bin in binaries)
            {
                Console.WriteLine($"({++n}/{binaries.Length}) {bin.Name}");
                Stopwatch sw = Stopwatch.StartNew();
                GZipCachedDownloader d = new GZipCachedDownloader();
                var cached = d.CacheGZip(bin.Name, bin.Url);
                Console.WriteLine($"  --> '{cached}', {sw.Elapsed}");

                FioVersionReader vr = new FioVersionReader(cached);
                sw.Reset();
                try
                {
                    var ver = vr.GetTextVersion();
                    Console.WriteLine($"  --> Version for {Path.GetFileName(cached)} [{ver}], {sw.Elapsed}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  --> Version Error [{(ex.GetType().Name + " " + ex.Message)}], {sw.Elapsed}");
                }
            }
        }
    }
}