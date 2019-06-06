using System;
using Microsoft.EntityFrameworkCore;
using Universe.Benchmark.DiskBench;
using Universe.DiskBench;

namespace Universe.Dashboard.DAL
{
    public class DashboardContext : DbContext
    {

        public DbSet<DbInfo> DbInfo { get; set; }
        public DbSet<HistoryCopy> HistoryCopy { get; set; }
        
        public DbSet<DiskBenchmarkEntity> DiskBenchmark { get; set; }

        public DashboardContext() : base(DashboardContextOptions.DesignTimeOptions)
        {
            // Console.WriteLine("Warning! DashboardContext() (default constructor is for design-time only)");
        }

        public DashboardContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            {
                var e = modelBuilder.Entity<DiskBenchmarkEntity>();
                e.Property(x => x.Args).HasConversion(JsonDbConverter.Create<DiskBenchmarkOptions>());
                e.Property(x => x.Report).HasConversion(JsonDbConverter.Create<ProgressInfo>());
                // e.HasIndex(p => new {p.Token}).IsUnique();
            }
        }
    }
}
