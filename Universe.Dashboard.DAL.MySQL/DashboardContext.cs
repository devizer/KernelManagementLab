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

        public DashboardContext() : base(DashboardContextOptions4MySQL.DesignTimeOptions)
        {
            // Console.WriteLine("Warning! DashboardContext() (default constructor is for design-time only)");
        }

        public DashboardContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            bool isMySQL = true;

            // Disk Benchmark Entity
            {
                var e = modelBuilder.Entity<DiskBenchmarkEntity>();
                e.Property(x => x.Args).HasConversion(JsonDbConverter.Create<DiskBenchmarkOptions>());
                e.Property(x => x.Report).HasConversion(JsonDbConverter.Create<ProgressInfo>());
                // e.HasIndex(p => new {p.Token}).IsUnique();
                
                if (isMySQL)
                {
                    e.Property(x => x.Token).HasColumnType("VARCHAR(36)");
                    e.Property(x => x.MountPath).HasColumnType("VARCHAR(20000)");
                    e.Property(x => x.Args).HasColumnType("LONGTEXT");
                    e.Property(x => x.Report).HasColumnType("LONGTEXT");
                }
            }

            // History Copy Entity
            {
                var e = modelBuilder.Entity<HistoryCopy>();
                
                if (isMySQL)
                {
                    e.Property(x => x.Key).HasColumnType("VARCHAR(20000)");
                    e.Property(x => x.JsonBlob).HasColumnType("LONGTEXT");
                }
            }

            // Db Info Entity
            {
                var e = modelBuilder.Entity<DbInfo>();
                
                if (isMySQL)
                {
                    e.Property(x => x.Version).HasColumnType("VARCHAR(20000)");
                }
            }

        }
    }
}
