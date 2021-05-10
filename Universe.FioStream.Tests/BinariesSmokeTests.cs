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
        public void Test_Download_Only()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
            {
                var url32 = "https://master.dl.sourceforge.net/project/fio/fio-3.25-x86-windows.exe.gz?viasf=1";
                var url64 = "https://master.dl.sourceforge.net/project/fio/fio-3.25-x64-windows.exe.gz?viasf=1";
                List<string> urls = new List<string>();
                urls.Add(url32);
                if (IntPtr.Size == 8) urls.Add(url64);
                foreach (var url in urls)
                {
                    var name = Path.GetFileNameWithoutExtension(new Uri(url).AbsolutePath);
                    Console.WriteLine(name);
                    Stopwatch sw = Stopwatch.StartNew();
                    GZipCachedDownloader d = new GZipCachedDownloader();
                    var cached = d.CacheGZip(name, url);
                    Console.WriteLine($"  --> '{cached}', {sw.Elapsed}");
                }
                
            }
        }
    }
}