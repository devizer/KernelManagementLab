using System;
using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Universe.Dashboard.DAL.MultiProvider
{
    public class PgSqlProvider4Runtime : IProvider4Runtime
    {
        public EF.Family Family => EF.Family.PgSql;
        
        public void ValidateConnectionString(string connectionString)
        {
            NpgsqlConnectionStringBuilder b = new NpgsqlConnectionStringBuilder(connectionString);
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        public void ApplyDbContextOptions(DbContextOptionsBuilder builder, string connectionString)
        {
            builder.UseNpgsql(connectionString, options =>
            {
                options.MigrationsHistoryTable(DashboardContextOptionsFactory.MigrationsTableName);
            });
        }

        public string GetShortVersion(IDbConnection connection)
        {
            return connection.ExecuteScalar<string>("show server_version;");
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

        public void CreateMigrationHistoryTableIfAbsent(IDbConnection connection, string migrationsHistoryTable)
        {
            var sqlSelect = string.Format(SqlSelectHistoryTable_PgSQL, migrationsHistoryTable);
            var existingHistoryTable = connection.ExecuteScalar<string>(sqlSelect);
            Console.WriteLine($@"existingHistoryTable: [{existingHistoryTable}], Query
{sqlSelect}");
            if (existingHistoryTable == null)
            {
                var sqlCreate = string.Format(SqlCreateHistoryTable_PgSQL, migrationsHistoryTable);
                connection.Execute(sqlCreate);
            }
        }
    }
}