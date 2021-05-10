using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        [TestCase(true,  TestName = "1. No Cache")]
        [TestCase(false, TestName = "2. Allow Cache")]
        public void Test_Download_Only(bool ignoreCache)
        {
            GZipCachedDownloader.IgnoreCacheForDebug = ignoreCache;
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
            {
                var binaries = Candidates.AllWindowsCandidates();
                
                foreach (var bin in binaries)
                {
                    Console.WriteLine(bin.Name);
                    Stopwatch sw = Stopwatch.StartNew();
                    GZipCachedDownloader d = new GZipCachedDownloader();
                    var cached = d.CacheGZip(bin.Name, bin.Url);
                    Console.WriteLine($"  --> '{cached}', {sw.Elapsed}");
                }
                
            }
        }
    }
}