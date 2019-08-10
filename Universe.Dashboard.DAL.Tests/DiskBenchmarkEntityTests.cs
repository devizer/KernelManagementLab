using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Dapper;
using KernelManagementJam;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using SQLitePCL;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;
using Universe.Dashboard.DAL.Tests;
using EF = Universe.Dashboard.DAL.MultiProvider.EF;

namespace Tests
{
    public class DiskBenchmarkEntityTests : NUnitTestsBase
    {
        static DashboardContext CreateSqlLiteDbContext() => DbTestEnv.CreateSqliteDbContext();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DashboardContext context = CreateSqlLiteDbContext();
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
        [TestCaseSource(typeof(DbTestEnv), nameof(DbTestEnv.TestParameters))]
        public void Test_Environment_Column(DbTestParameter argDB)
        {
            var token = Guid.NewGuid();
            var expectedFileSystem = "myfs";
            
            DashboardContext context = argDB.GetDashboardContext();
            DiskBenchmark b = new DiskBenchmark("/test-empty-report");
            var entity = new DiskBenchmarkEntity()
            {
                CreatedAt = DateTime.UtcNow, 
                MountPath = b.Parameters.WorkFolder,
                Token = token,
                Environment = new DiskbenchmarkEnvironment { FileSystems = expectedFileSystem }, 
            };
            entity.Args = b.Parameters;
            entity.Report = b.Progress;
            context.DiskBenchmark.Add(entity);
            context.SaveChanges();

            var copy = context.DiskBenchmark.FirstOrDefault(x => x.Token == token);
            Assert.IsNotNull(copy, "entity found by token");
            Assert.IsNotNull(copy.Environment, "entity.Environment != null");
            Assert.AreEqual(expectedFileSystem, copy.Environment.FileSystems, "entity.Environment.FileSystem is received");
        }


    }
}
