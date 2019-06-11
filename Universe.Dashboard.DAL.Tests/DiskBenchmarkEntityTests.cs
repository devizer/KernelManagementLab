using System;
using KernelManagementJam.Benchmarks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL;

namespace Tests
{
    public class DiskBenchmarkEntityTests
    {
        static DashboardContext CreateDbContext() => DbEnv.CreateDbContext();
        
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
            context.DiskBenchmark.Add(new DiskBenchmarkEntity() { CreatedAt = DateTime.UtcNow, MountPath = "/test-empty"});
            context.SaveChanges();
        }

        [Test]
        public void TestArguments()
        {
            DashboardContext context = CreateDbContext();
            DiskBenchmark b = new DiskBenchmark("/test-args");
            var entity = new DiskBenchmarkEntity() { CreatedAt = DateTime.UtcNow, MountPath = b.Parameters.WorkFolder};
            entity.Args = b.Parameters;
            context.DiskBenchmark.Add(entity);
            context.SaveChanges();
        }

        [Test]
        public void TestEmptyReport()
        {
            DashboardContext context = CreateDbContext();
            DiskBenchmark b = new DiskBenchmark("/test-empty-report");
            var entity = new DiskBenchmarkEntity() { CreatedAt = DateTime.UtcNow, MountPath = b.Parameters.WorkFolder};
            entity.Args = b.Parameters;
            entity.Report = b.Progress;
            context.DiskBenchmark.Add(entity);
            context.SaveChanges();
        }
        
        [Test]
        public void Test_REAL()
        {
            DashboardContext context = CreateDbContext();
            DiskBenchmark b = new DiskBenchmark(".", 128*1024,DataGeneratorFlavour.Random, 4096, 1);
            b.Perform();
            var entity = new DiskBenchmarkEntity() { CreatedAt = DateTime.UtcNow, MountPath = b.Parameters.WorkFolder};
            entity.Args = b.Parameters;
            entity.Report = b.Progress;
            context.DiskBenchmark.Add(entity);
            context.SaveChanges();
            
        }

    }
}
