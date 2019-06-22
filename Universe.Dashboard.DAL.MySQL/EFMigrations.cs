using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public class EFMigrations
    {
        private const string SqlSelectHistoryTable = @"
SELECT table_name 
FROM information_schema.tables
WHERE table_schema = DATABASE() AND table_name = 'UpgradeHistory'
LIMIT 1;
";

        private const string SqlCreateHistoryTable = @"
CREATE TABLE UpgradeHistory ( 
  MigrationId varchar(150) NOT NULL PRIMARY KEY, 
  ProductVersion varchar(32) NOT NULL 
) CHARSET=utf8;";
        
        public static void Migrate_MySQL(DbContext context, string migrationsHistoryTable = "__EFMigrationsHistory")
        {
            // context.Database.EnsureCreated();
            var existingHistoryTable = context.Database.GetDbConnection().ExecuteScalar<string>(SqlSelectHistoryTable);
            if (existingHistoryTable == null)
            {
                context.Database.GetDbConnection().Execute(SqlCreateHistoryTable);
            }
            
            context.Database.Migrate();
        }
        
    }
}