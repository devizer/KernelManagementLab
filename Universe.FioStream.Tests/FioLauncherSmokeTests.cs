using System;
using System.IO;
using NUnit.Framework;
using Tests;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class FioLauncherSmokeTests : NUnitTestsBase
    {
        private const string DefaultFio = "fio";

        [Test]
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
        public void SmokeLaunch()
        {
            var args =
                "--ioengine=sync --name=aaa --eta=always --time_based --filename=fiotest.tmp --bs=4k --iodepth=64 --size=64k --runtime=2 --ramp_time=2 --readwrite=randread"
                    .Split(' ');
            
            Action<StreamReader> handler = streamReader =>
            {
                FioStreamReader rdr = new FioStreamReader();
                rdr.NotifyEta += eta => Console.WriteLine($"ETA: [{eta}]");
                rdr.NotifyJobProgress += progress => Console.WriteLine($"PROGRESS: [{progress}]");
                rdr.NotifyJobSummary += summary => Console.WriteLine($"SUMMARY: [{summary}]");
                rdr.ReadStreamToEnd(streamReader);
            };
            FioLauncher launcher = new FioLauncher(DefaultFio, args, handler);
            launcher.Start();
            if (!string.IsNullOrEmpty(launcher.ErrorText))
            {
                Console.WriteLine($"ERROR TEXT: {launcher.ErrorText}");
            }
        }
    }
}