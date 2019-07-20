using System;
using Microsoft.EntityFrameworkCore;
using Universe.Dashboard.DAL.MultiProvider;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptionsFactory
    {
        public static class EnvNames
        {
            public static readonly string PgSqlDb = "PGSQL_DATABASE";
            public static readonly string MySqlDb = "MYSQL_DATABASE";
        }

        public static string MigrationsTableName = "W3Top_MigrationsHistory";

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
            
            if (f == EF.Family.MySql) 
                optionsBuilder.ApplyMySqlOptions(DashboardContextOptions4MySQL.ConnectionString);
            
            else if (f == EF.Family.PgSql)
                optionsBuilder.ApplyPgSqlOptions(DashboardContextOptions4PgSQL.ConnectionString);

            else
                optionsBuilder.ApplySqliteOptions(SqliteDatabaseOptions.DbFullPath);
            
        }

        public static EF.Family Family => RuntimeParameters.Family;
/*
        {
            get
            {
                if (!string.IsNullOrEmpty(DashboardContextOptions4MySQL.ConnectionString))
                    return EF.Family.MySql;
                
                if (!string.IsNullOrEmpty(DashboardContextOptions4PgSQL.ConnectionString))
                    return EF.Family.PgSql;

                return EF.Family.Sqlite;  
            }
        }
*/

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