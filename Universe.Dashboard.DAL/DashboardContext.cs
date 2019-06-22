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

        public DashboardContext() : base(DashboardContextOptionsFactory.Create())
        {
            // Console.WriteLine("Warning! DashboardContext() (default constructor is for design-time only)");
        }

        public DashboardContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Console.WriteLine($"{nameof(DashboardContext)}::CONFIGURING");
            base.OnConfiguring(optionsBuilder);
            // Console.WriteLine($"{nameof(DashboardContext)}::CONFIGURED");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var types = DashboardContextOptionsFactory.Family.GetTypes();

            // Disk Benchmark Entity
            {
                var e = modelBuilder.Entity<DiskBenchmarkEntity>();
                e.Property(x => x.Args).HasConversion(JsonDbConverter.Create<DiskBenchmarkOptions>());
                e.Property(x => x.Report).HasConversion(JsonDbConverter.Create<ProgressInfo>());
                e.HasIndex(p => new {p.Token}).IsUnique();
                e.Property(x => x.Token).HasColumnType(types.Guid);
                e.Property(x => x.MountPath).HasColumnType(types.String);
                e.Property(x => x.Args).HasColumnType(types.Json);
                e.Property(x => x.Report).HasColumnType(types.Json);
            }

            // History Copy Entity
            {
                var e = modelBuilder.Entity<HistoryCopy>();
                e.Property(x => x.Key).HasColumnType(types.String);
                e.Property(x => x.JsonBlob).HasColumnType(types.Json);
            }

            // Db Info Entity
            {
                var e = modelBuilder.Entity<DbInfo>();
                // e.Property(x => x.Version).HasColumnType(types.String);
            }
        }
    }
}
