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
            var binaries = OrderedLinuxCandidates.GetLinuxCandidates();
                
            foreach (var bin in binaries)
            {
                Console.WriteLine(bin);
            }
        }

        [Test]
        [TestCase(true, TestName = "1. No Cache")]
        [TestCase(false, TestName = "2. Allow Cache")]
        public void Test_Download_Only(bool ignoreCache)
        {
            GZipCachedDownloader.IgnoreCacheForDebug = ignoreCache;
            var binaries = Candidates.AllWindowsCandidates()
                .Concat(Candidates.AllMacOsCandidates())
                .Concat(OrderedLinuxCandidates.GetLinuxCandidates().Select(x => new Candidates.Info()
                {
                    Name = x.Name,
                    Url = x.Url,
                })).ToArray();

            foreach (var bin in binaries)
            {
                Console.WriteLine(bin.Name);
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