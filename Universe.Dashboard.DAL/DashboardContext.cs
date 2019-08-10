using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MySql.Data.EntityFrameworkCore.Extensions;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.DAL.MultiProvider;
using Universe.DiskBench;
using EF = Universe.Dashboard.DAL.MultiProvider.EF;

namespace Universe.Dashboard.DAL
{
    public class DashboardContext : DbContext
    {

        public DbSet<DbInfoEntity> DbInfo { get; set; }
        public DbSet<HistoryCopyEntity> HistoryCopy { get; set; }
        
        public DbSet<DiskBenchmarkEntity> DiskBenchmark { get; set; }

        public DashboardContext() : base(DashboardContextOptionsFactory.Create())
        {
            // Console.WriteLine("Warning! DashboardContext() (default constructor is for design-time only)");
        }

        public DashboardContext(DbContextOptions options) : base(options)
        {
        }

        private static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        private static readonly LogLevel[] ReleaseLevels = new[] {LogLevel.Error, LogLevel.Warning, LogLevel.Critical};
        
        public static readonly ILoggerFactory loggerFactory = new LoggerFactory(new[] {
            new ConsoleLoggerProvider((_, level) => IsDebug || ReleaseLevels.Contains(level) || DashboardContextOptionsFactory.Family != EF.Family.Sqlite, true)
        });
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Console.WriteLine($"{nameof(DashboardContext)}::CONFIGURING");
            base.OnConfiguring(optionsBuilder);
            // Console.WriteLine($"{nameof(DashboardContext)}::CONFIGURED");

            optionsBuilder.UseLoggerFactory(loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var types = Database.GetFamily().GetTypes();

            // Disk Benchmark Entity
            {
                var e = modelBuilder.Entity<DiskBenchmarkEntity>();
                e.Property(x => x.Args).HasConversion(JsonDbConverter.Create<DiskBenchmarkOptions>());
                e.Property(x => x.Report).HasConversion(JsonDbConverter.Create<ProgressInfo>());
                e.HasIndex(p => new {p.Token}).IsUnique();
                e.Property(x => x.Token).HasColumnType(types.Guid);
                e.Property(x => x.CreatedAt).HasColumnType(types.DateTime).HasDefaultValueSql(types.CurrentDateTime);
                e.Property(x => x.MountPath).HasColumnType(types.String);
                e.Property(x => x.Args).HasColumnType(types.Json);
                e.Property(x => x.Report).HasColumnType(types.Json);
                
                e.Property(x => x.Environment)
                    .HasColumnType(types.Json)
                    .HasConversion(JsonDbConverter.Create<DiskbenchmarkEnvironment>());
            }

            // History Copy Entity
            {
                var e = modelBuilder.Entity<HistoryCopyEntity>();
                e.Property(x => x.Key).HasColumnType(types.String);
                e.Property(x => x.JsonBlob).HasColumnType(types.Json);
            }

            // Db Info Entity
            {
                var e = modelBuilder.Entity<DbInfoEntity>();
                e.Property(x => x.Version).HasColumnType(types.String);
            }

        }
    }
}
