using System;
using KernelManagementJam.Benchmarks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;
using Universe.Dashboard.DAL.Tests;

namespace Tests
{
    public class DiskBenchmark_Normal_Lifetime_Tests : NUnitTestsBase
    {
        

        [Test]
        [TestCaseSource(typeof(DbTestEnv), nameof(DbTestEnv.TestParameters))]
        public void Perform_Save_Fetch(DbTestParameter argDB)
        {
            ShowDbTestArgument(argDB);
            if (IsDebug) Environment.SetEnvironmentVariable("SKIP_FLUSHING", "true");
            
            DashboardContext context = argDB.GetDashboardContext();
            Console.WriteLine($"Provider: [{context.Database.ProviderName}]");
            Console.WriteLine($"Connection String: [{context.Database.GetDbConnection().ConnectionString}]");
            DiskBenchmark b = new DiskBenchmark(".", 128*1024,DataGeneratorFlavour.Random, 4096, 1);
            b.Perform();
            var entity = new DiskBenchmarkEntity()
            {
                CreatedAt = DateTime.UtcNow, 
                MountPath = b.Parameters.WorkFolder,
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
            Assert.AreEqual(copyByToken.Report.Steps.Count, entity.Report.Steps.Count);
            Console.WriteLine("DiskBenchmarkDataAccess.GetDiskBenchmarkResult by token is complete");
        }
        
        void ShowDbTestArgument(DbTestParameter arg)
        {
            using (var db = arg.GetDashboardContext())
            {
                var connection = db.Database.GetDbConnection();
                Console.WriteLine($@"Arg.Family: ............... {arg.Family}
Arg.Provider .............. [{db.Database.ProviderName}]
Arg.DB.ConnectionString ... [{connection.ConnectionString}]
Arg.DB.SelectVersion() .... [{db.Database.GetShortVersion()}]
");
            }
        }

    }
}