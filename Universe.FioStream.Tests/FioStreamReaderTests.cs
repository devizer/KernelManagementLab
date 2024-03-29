using System;
using System.Linq;
using AutoGeneratedTests;
using NUnit.Framework;
using Universe.NUnitTests;

namespace Universe.FioStream.Tests
{
    public class FioStreamReaderTests /*: NUnitTestsBase*/
    {
        [SetUp]
        public void SetUp() => FioStreamReader.ConsolasDebug = true;

        [Test]
        public void _0_Info()
        {
            Console.WriteLine($"FioParserTestCase.All.Length: {FioParserTestCase.All.Length}");
            Assert.Pass();
        }

        [Test]
        public void _0_Info_V3()
        {
            Console.WriteLine($"FioParserTestCase2.GetAll_V3().Count(): {FioParserTestCase2.GetAll_V3().Count()}");
            Assert.Pass();
        }

        [Test, TestCaseSource(typeof(FioParserTestCase2), nameof(FioParserTestCase2.GetAll_V3))]
        public void FullFio(FioParserTestCase2 testCase)
        {
            const bool DEBUG = false;
            FioStreamReader.ConsolasDebug = DEBUG;

            JobSummaryResult jobSummaryResult = null;
            int jobSummaryResultCount = 0;
            JobSummaryCpuUsage jobSummaryCpuUsage = null;
            FioStreamReader reader = new FioStreamReader();
            reader.NotifyJobSummary += result =>
            {
                if (DEBUG) Console.WriteLine($"[JobSummaryResult Structured]: {result}");
                jobSummaryResult = result;
                jobSummaryResultCount++;
            };

            reader.NotifyJobSummaryCpuUsage += cpuUsage =>
            {
                if (DEBUG) Console.WriteLine($"[JobSummaryCpuUsage Structured]: {cpuUsage}");
                jobSummaryCpuUsage = cpuUsage;
            };

            reader.NotifyEta += eta =>
            {
                if (DEBUG) Console.WriteLine($"[ETA Structured]: {eta}");
            };

            bool isFirst = true;
            double? prevPerCents = null;
            bool? hasIops = null, hasBandwidth = null;
            reader.NotifyJobProgress += jobProgress =>
            {
                if (DEBUG) Console.WriteLine($"JobProgress: {jobProgress}");
                if (testCase.Version != "2.2.12")
                    Assert.IsTrue(jobProgress.Eta.HasValue, "ETA is not null");
                
                if (jobProgress.PerCents.HasValue)
                {
                    if (prevPerCents.GetValueOrDefault() <= 100.0)
                    {
                        if (prevPerCents.HasValue)
                        {
                            Assert.IsTrue(jobProgress.PerCents.HasValue, "Prev PerCents less then next");
                        }
                        prevPerCents = jobProgress.PerCents;
                    }
                }
                // Assert.IsTrue(jobProgress.PerCents.HasValue, "PerCents is not null");
                // Assert.IsTrue(jobProgress.PerCents.Value > 0, "PerCents bigger then zero");
                var currentHasIops = jobProgress.ReadIops.GetValueOrDefault() > 0 || jobProgress.WriteIops.GetValueOrDefault() > 0;
                var currentHasBandwidth = jobProgress.ReadBandwidth.GetValueOrDefault() > 0 || jobProgress.WriteBandwidth.GetValueOrDefault() > 0;
                if (!isFirst)
                {
                    Assert.IsTrue(currentHasIops, "Either Read IOPS or Write IOPS bigger then zero for progress"); 
                    Assert.IsTrue(currentHasBandwidth, "Either Read Bandwidth or Write Bandwidth bigger then zero for progress"); 
                }

                if (currentHasIops) hasIops = true;
                if (currentHasBandwidth) hasBandwidth = true;

                isFirst = false;
            };
            
            foreach (var line in testCase.Lines)
            {
                reader.ReadNextLine(line);
            }
            
            // Summary
            Assert.NotNull(jobSummaryResult, "FioStreamReader should provide JobSummaryResult");
            Assert.AreEqual(testCase.NumJobs, jobSummaryResultCount, $"jobSummaryResultCount == {testCase.NumJobs}");
            Assert.True(jobSummaryResult.Iops > 0, "JobSummaryResult.Iops should be greater then zero");
            Assert.True(jobSummaryResult.Bandwidth > 0, "JobSummaryResult.Bandwidth should be greater then zero");
            
            Assert.NotNull(jobSummaryCpuUsage, "FioStreamReader should provide JobSummaryCpuUsage");
            if (testCase.Version != "3.0")
                Assert.True(jobSummaryCpuUsage.UserPercents + jobSummaryCpuUsage.KernelPercents > 0, "jobSummaryCpuUsage.UserPercents + jobSummaryCpuUsage.KernelPercents should be greater then zero");
            
            

            // Progress
            if (/*testCase.Version != "2.11" && */testCase.Version != "3.0")
            {
                Assert.IsTrue(prevPerCents.HasValue, "PerCents arrived on progress");
            }

            if (testCase.Version != "3.0")
            {
                Assert.IsTrue(hasIops.GetValueOrDefault(), "IOPS Arrived on progress");
                Assert.IsTrue(hasBandwidth.GetValueOrDefault(), "Bandwidth Arrived");
            }

        }

    }
}