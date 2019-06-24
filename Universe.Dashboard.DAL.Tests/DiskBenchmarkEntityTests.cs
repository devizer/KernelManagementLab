using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Dapper;
using KernelManagementJam;
using KernelManagementJam.Benchmarks;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using SQLitePCL;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;
using EF = Universe.Dashboard.DAL.EF;

namespace Tests
{
    public class DiskBenchmarkEntityTests : NUnitTestsBase
    {
        static DashboardContext CreateSqlLiteDbContext() => DbEnv.CreateSqliteDbContext();

        static DiskBenchmarkEntityTests()
        {
            Console.WriteLine("static DiskBenchmarkEntityTests.ctor");
            OUT = Console.Out;
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DashboardContext context = DiskBenchmarkEntityTests.CreateSqlLiteDbContext();
            context.Database.Migrate();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Console.WriteLine("DiskBenchmarkEntityTests::OneTimeTearDown - nothing todo");
        }

        [Test]
        public void TestEmpty()
        {
            DashboardContext context = CreateSqlLiteDbContext();
            context.DiskBenchmark.Add(new DiskBenchmarkEntity()
            {
                CreatedAt = DateTime.UtcNow, 
                MountPath = "/test-empty",
                Token = Guid.NewGuid()
            });
            context.SaveChanges();
        }

        [Test]
        public void TestArguments()
        {
            DashboardContext context = CreateSqlLiteDbContext();
            DiskBenchmark b = new DiskBenchmark("/test-args");
            var entity = new DiskBenchmarkEntity()
            {
                CreatedAt = DateTime.UtcNow, 
                MountPath = b.Parameters.WorkFolder,
                Token = Guid.NewGuid(),
                    
            };
            entity.Args = b.Parameters;
            context.DiskBenchmark.Add(entity);
            context.SaveChanges();
        }

        [Test]
        public void TestEmptyReport()
        {
            DashboardContext context = CreateSqlLiteDbContext();
            DiskBenchmark b = new DiskBenchmark("/test-empty-report");
            var entity = new DiskBenchmarkEntity()
            {
                CreatedAt = DateTime.UtcNow, 
                MountPath = b.Parameters.WorkFolder,
                Token = Guid.NewGuid(),
            };
            entity.Args = b.Parameters;
            entity.Report = b.Progress;
            context.DiskBenchmark.Add(entity);
            context.SaveChanges();
        }

        [Test]
        public void MySQL_Environment_Info()
        {
            var msg = $@"MySqlTestEnv.NeedMySqlTests: {MySqlTestEnv.NeedMySqlTests} 
MySQL Admin's connection: [{MySqlTestEnv.AdminConnectionString}]";
            
            Console.WriteLine(msg);
        }

        [Test]
        [TestCaseSource(typeof(DbEnv), nameof(DbEnv.TestParameters))]
        public void Test_DB_Args(DbParameter argDB)
        {
            using (var db = argDB.GetDashboardContext())
            {
                var connection = db.Database.GetDbConnection();
                Console.WriteLine($@"Arg.Family: {argDB.Family}
Arg.Provider [{db.Database.ProviderName}]
Arg.DB.ConnectionString [{connection.ConnectionString}]");

                var version = "<unknwon>";
                try
                {
                    var sql = argDB.Family == EF.Family.Sqlite ? "sqlite_version()" : "version()";
                    version = connection.ExecuteScalar<string>($"Select {sql};");
                }
                catch (Exception ex)
                {
                    version = ex.GetExceptionDigest();
                }

                Console.WriteLine($@"Arg.DB.SelectVersion(): [{version}]");
            }
        }

        [Test]
        [TestCaseSource(typeof(DbEnv), nameof(DbEnv.TestParameters))]
        public void Test_REAL(DbParameter argDB)
        {
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

    }
}
