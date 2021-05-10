using System;
using System.IO;
using NUnit.Framework;
using Tests;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class FioLauncherSmokeTests : NUnitTestsBase
    {
        [SetUp]
        public void SetUp() => FioStreamReader.ConsolasDebug = false;
        
        private const string DefaultFio = "fio";

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
            var args =
                $"--ioengine=sync --name=my --eta=always --filename=fiotest.tmp --iodepth=1 --readwrite=read {additionalArgs}"
                    .Split(' ');

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
    }
}