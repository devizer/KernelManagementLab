using System;
using System.Diagnostics;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public class EFMigrations
    {
        private const string SqlSelectHistoryTable = @"
SELECT table_name 
FROM information_schema.tables
WHERE table_schema = DATABASE() AND table_name = '{0}'
LIMIT 1;
";

        private const string SqlCreateHistoryTable = @"
CREATE TABLE {0} ( 
  MigrationId varchar(150) NOT NULL PRIMARY KEY, 
  ProductVersion varchar(32) NOT NULL 
) CHARSET=utf8;";
        
        public static void Migrate_MySQL(DbContext context, string migrationsHistoryTable = "__EFMigrationsHistory")
        {
            // context.Database.EnsureCreated();
            var sqlSelect = string.Format(SqlSelectHistoryTable, migrationsHistoryTable);
            var existingHistoryTable = context.Database.GetDbConnection().ExecuteScalar<string>(sqlSelect);
            if (existingHistoryTable == null)
            {
                var sqlCreate = string.Format(SqlCreateHistoryTable, migrationsHistoryTable);
                context.Database.GetDbConnection().Execute(sqlCreate);
            }
            
            context.Database.Migrate();
        }
    
    }

    public class EFHealth
    {
        public static Exception WaitFor(DbContext db, int timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Exception ret = null;
            do
            {
                try
                {
                    db.Database.GetDbConnection().Execute("Select null;");
                    return null;
                }
                catch (Exception ex)
                {
                    ret = ex;
                }

            } while (sw.ElapsedMilliseconds < timeout);

            return ret;
        }
    }
}
