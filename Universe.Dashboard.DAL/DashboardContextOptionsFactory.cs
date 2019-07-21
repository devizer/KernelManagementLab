using System;
using Microsoft.EntityFrameworkCore;
using Universe.Dashboard.DAL.MultiProvider;
using EF = Universe.Dashboard.DAL.MultiProvider.EF;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptionsFactory
    {
        public static string MigrationsTableName = "W3Top_MigrationsHistory";

        public static class EnvNames
        {
            public static readonly string PgSqlDb = "PGSQL_DATABASE";
            public static readonly string MySqlDb = "MYSQL_DATABASE";
        }

        public static DbContextOptions Create()
        {
            return new DbContextOptionsBuilder().ApplyOptions().Options;
        }
        
        public static DbContextOptionsBuilder ApplyOptions(this DbContextOptionsBuilder optionsBuilder)
        {
            var runtimeParameters = RuntimeParameters;
            var f = runtimeParameters.Family;
            Console.WriteLine($"F*A*M*I*L*Y: {f}");
            f.GetProvider().ApplyDbContextOptions(optionsBuilder, runtimeParameters.ConnectionString);
            optionsBuilder.EnableSensitiveDataLogging(true);
            return optionsBuilder;
        }

        public static EF.Family Family => RuntimeParameters.Family;

        public static DashboardContextRuntimeParameters RuntimeParameters
        {
            get
            {
                var mySql = Environment.GetEnvironmentVariable(EnvNames.MySqlDb);
                if (!string.IsNullOrEmpty(mySql))
                    return new DashboardContextRuntimeParameters
                    {
                        Family = EF.Family.MySql,
                        ConnectionString = mySql
                    };

                var pgSql = Environment.GetEnvironmentVariable(EnvNames.PgSqlDb);
                if (!string.IsNullOrEmpty(pgSql))
                    return new DashboardContextRuntimeParameters
                    {
                        Family = EF.Family.PgSql,
                        ConnectionString = pgSql
                    };

                return new DashboardContextRuntimeParameters
                {
                    Family = EF.Family.Sqlite,
                    ConnectionString = $"DataSource={SqliteDatabaseOptions.DbFullPath}"
                };
            }
        }
    }
}