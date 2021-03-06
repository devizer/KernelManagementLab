using System;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Universe.Dashboard.DAL.MultiProvider;

namespace Universe.Dashboard.DAL
{
    [Obsolete("Use IProvider4Runtime")]
    public class EFMigrations_Legacy
    {
        private const string SqlSelectHistoryTable_MySQL = @"
SELECT table_name 
FROM information_schema.tables
WHERE table_schema = DATABASE() AND table_name = '{0}'
LIMIT 1;
";

        private const string SqlCreateHistoryTable_MySQL = @"
CREATE TABLE {0} ( 
  MigrationId varchar(150) NOT NULL PRIMARY KEY, 
  ProductVersion varchar(32) NOT NULL 
) CHARSET=utf8;";

        
        public static void Migrate_MySQL(DbContext context, string migrationsHistoryTable = "__EFMigrationsHistory")
        {
            var family = context.Database.GetFamily();
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
            Console.WriteLine($"Applying [{family}] migrations if required. Server: {b.Server}. Port: {b.Port}");
            
            // context.Database.EnsureCreated();
            var sqlSelect = string.Format(SqlSelectHistoryTable_MySQL, migrationsHistoryTable);
            var existingHistoryTable = context.Database.GetDbConnection().ExecuteScalar<string>(sqlSelect);
            if (existingHistoryTable == null)
            {
                var sqlCreate = string.Format(SqlCreateHistoryTable_MySQL, migrationsHistoryTable);
                context.Database.GetDbConnection().Execute(sqlCreate);
            }
            
            context.Database.Migrate();
        }

        private const string SqlSelectHistoryTable_PgSQL = @"
SELECT table_name 
FROM information_schema.tables
WHERE table_catalog = CURRENT_DATABASE() AND table_name = '{0}'
LIMIT 1;
";

        private const string SqlCreateHistoryTable_PgSQL = @"
CREATE TABLE ""{0}"" ( 
  ""MigrationId"" varchar(150) NOT NULL PRIMARY KEY, 
  ""ProductVersion"" varchar(32) NOT NULL 
)";

        public static void Migrate_PgSQL(DbContext context, string migrationsHistoryTable = "__EFMigrationsHistory")
        {
            // throw new NotImplementedException();
            // context.Database.EnsureCreated();
            var sqlSelect = string.Format(SqlSelectHistoryTable_PgSQL, migrationsHistoryTable);
            var existingHistoryTable = context.Database.GetDbConnection().ExecuteScalar<string>(sqlSelect);
            Console.WriteLine($@"existingHistoryTable: [{existingHistoryTable}], Query
{sqlSelect}");
            if (existingHistoryTable == null)
            {
                var sqlCreate = string.Format(SqlCreateHistoryTable_PgSQL, migrationsHistoryTable);
                context.Database.GetDbConnection().Execute(sqlCreate);
            }
            
            context.Database.Migrate();
        }
    
    }
}
