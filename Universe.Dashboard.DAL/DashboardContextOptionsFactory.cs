using System;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptionsFactory
    {
        public static string MigrationsTableName = "W3Top_MigrationsHistory";

        public static DbContextOptions Create()
        {
            return new DbContextOptionsBuilder().ApplyOptions().Options;
        }
        
        public static DbContextOptionsBuilder ApplyOptions(this DbContextOptionsBuilder optionsBuilder)
        {
            var f = Family;
            Console.WriteLine($"FFFAAAMMMIIILLLYYY: {f}");
            if (f == EF.Family.MySql) 
                optionsBuilder.ApplyMySqlOptions(DashboardContextOptions4MySQL.ConnectionString);
            
            else if (f == EF.Family.PgSql)
                optionsBuilder.ApplyPgSqlOptions(DashboardContextOptions4PgSQL.ConnectionString);

            else
                optionsBuilder.ApplySqliteOptions(SqliteDatabaseOptions.DbFullPath);
            
            optionsBuilder.EnableSensitiveDataLogging(true);
            return optionsBuilder;
        }


        public static EF.Family Family
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
    }
}