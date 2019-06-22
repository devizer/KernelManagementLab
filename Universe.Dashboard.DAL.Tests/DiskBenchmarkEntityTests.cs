using System;
using KernelManagementJam.Benchmarks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;
using EF = Universe.Dashboard.DAL.EF;

namespace Tests
{
    public class DiskBenchmarkEntityTests
    {
        static DashboardContext CreateDbContext() => DbEnv.CreateSqliteDbContext();
        
        [SetUp]
        public void Setup()
        {
            DashboardContext context = CreateDbContext();
            context.Database.Migrate();
        }

        [Test]
        public void TestEmpty()
        {
            DashboardContext context = CreateDbContext();
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
            DashboardContext context = CreateDbContext();
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
            DashboardContext context = CreateDbContext();
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
        [TestCaseSource(typeof(DbEnv), nameof(DbEnv.TestParameters))]
        public void Test_REAL(DbParameter dbArg)
        {
            if (dbArg.Family == EF.Family.Sqlite)
            {
                Environment.SetEnvironmentVariable("MYSQL_DATABASE", "");
            }
            
            DashboardContext context = dbArg.GetDashboard();
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
            context.DiskBenchmark.Add(entity);
            context.SaveChanges();
        }

    }
}
