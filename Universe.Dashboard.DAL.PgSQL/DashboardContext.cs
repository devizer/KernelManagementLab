using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MySql.Data.EntityFrameworkCore.Extensions;
using Universe.Benchmark.DiskBench;
using Universe.DiskBench;

namespace Universe.Dashboard.DAL
{
    public class DashboardContext : DbContext
    {

        public DbSet<DbInfo> DbInfo { get; set; }
        public DbSet<HistoryCopy> HistoryCopy { get; set; }
        
        public DbSet<DiskBenchmarkEntity> DiskBenchmark { get; set; }

        public DashboardContext() : base(temp())
        {
            
            // Console.WriteLine("Warning! DashboardContext() (default constructor is for design-time only)");
            
        }

        static DbContextOptions temp()
        {
            var cs = "Host=localhost;Database=w3top;Username=w3top;Password=pass";
            DbContextOptions ret = new DbContextOptionsBuilder()
                .ApplyPgSqlOptions(cs)
                .Options;

            return ret;


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

        protected /*override*/ void OnModelCreating_Ignore(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var types = Database.GetTypes();

            // Disk Benchmark Entity
            {
                var e = modelBuilder.Entity<DiskBenchmarkEntity>();
                e.Property(x => x.Args).HasConversion(JsonDbConverter.Create<DiskBenchmarkOptions>());
                e.Property(x => x.Report).HasConversion(JsonDbConverter.Create<ProgressInfo>());
                e.HasIndex(p => new {p.Token}).IsUnique();
/*
                e.Property(x => x.Token).HasColumnType(types.Guid);
                e.Property(x => x.CreatedAt).HasColumnType(types.DateTime).HasDefaultValueSql(types.CurrentDateTime);
                e.Property(x => x.MountPath).HasColumnType(types.String);
                e.Property(x => x.Args).HasColumnType(types.Json);
                e.Property(x => x.Report).HasColumnType(types.Json);
*/
            }

            // History Copy Entity
            {
                var e = modelBuilder.Entity<HistoryCopy>();
/*
                e.Property(x => x.Key).HasColumnType(types.String);
                e.Property(x => x.JsonBlob).HasColumnType(types.Json);
*/
            }

            // Db Info Entity
            {
                var e = modelBuilder.Entity<DbInfo>();
/*
                e.Property(x => x.Version).HasColumnType(types.String);
*/
            }
        }
    }
}
