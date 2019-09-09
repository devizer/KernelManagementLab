using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using KernelManagementJam.Benchmarks;
using KernelManagementJam.DebugUtils;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;
using Universe.Dashboard.DAL.MultiProvider;
using Universe.Dashboard.DAL.Tests;
using Universe.DiskBench;

namespace Tests
{
    public class DiskBenchmark_Normal_Flow_Tests : NUnitTestsBase
    {

        [Test]
        [TestCaseSource(typeof(DbTestEnv), nameof(DbTestEnv.TestDatabases))]
        public void Perform_Full_Lifecycle(TestDatabase arg)
        {
            ShowDbTestArgument(arg);
            Environment.SetEnvironmentVariable("SKIP_FLUSHING", "true");
            
            DashboardContext context = arg.GetDashboardContext();
            DiskBenchmark b = new DiskBenchmark(".", 128*1024,DataGeneratorFlavour.Random, 4096, 1);
            b.Perform();
            var entity = new DiskBenchmarkEntity()
            {
                CreatedAt = DateTime.UtcNow, 
                MountPath = b.Parameters.WorkFolder,
                IsSuccess = true,
                ErrorInfo = null,
                Token = Guid.NewGuid()
            };
            
            entity.Args = b.Parameters;
            entity.Report = b.Progress;
            if (context == null) throw new InvalidOperationException("argDB.GetDashboardContext() returns null");
            context.DiskBenchmark.Add(entity);
            context.SaveChanges();
            Console.WriteLine("SAVE to DB is complete");
            
            DiskBenchmarkDataAccess dbda = new DiskBenchmarkDataAccess(context);
            var copyByToken = dbda.GetDiskBenchmarkResult(entity.Token);
            // round double seconds
            {
                void DoRound(ProgressInfo progress) => progress.Steps.Where(step => step.Seconds.HasValue).ToList()
                    .ForEach(step => step.Seconds = Math.Round(step.Seconds.GetValueOrDefault(), 4));

                DoRound(copyByToken.Report);
                DoRound(entity.Report);
            }
            var actual = copyByToken.Report; var expected = entity.Report;
            Assert.AreEqual(expected.AsJson(), actual.AsJson());
            Console.WriteLine("DiskBenchmarkDataAccess.GetDiskBenchmarkResult by token is complete");

            List<DiskBenchmarkEntity> history = dbda.GetHistory();
            CollectionAssert.IsNotEmpty(history, "History should contain benchmark result");
            var copyFromHistory = history.FirstOrDefault(x => x.Token == entity.Token);
            Assert.IsNotNull(copyFromHistory, "copyFromHistory is not null");
            Assert.AreEqual(".", copyFromHistory.MountPath);
            var historyRow = copyFromHistory.ToHistoryItem();
            Assert.IsTrue(historyRow.SeqRead.GetValueOrDefault() > 0, "SeqRead > 0");
            Assert.IsTrue(historyRow.RandRead1T.GetValueOrDefault() > 0, "RandRead1T > 0");
            Assert.IsTrue(historyRow.RandReadNT.GetValueOrDefault() > 0, "RandReadNT > 0");
            Console.WriteLine("DiskBenchmarkDataAccess.GetHistory -> ToHistoryItem() works properly");

        }
        
        void ShowDbTestArgument(TestDatabase arg)
        {
            using (var db = arg.GetDashboardContext())
            {
                var connection = db.Database.GetDbConnection();
                Console.WriteLine($@"Arg.Family: ............... {arg.Family}
Arg.Provider .............. [{db.Database.ProviderName}]
Arg.DB.ConnectionString ... [{connection.ConnectionString}]
Arg.DB.SelectVersion() .... [{db.Database.GetShortVersion()}]");
            }
        }

    }
}