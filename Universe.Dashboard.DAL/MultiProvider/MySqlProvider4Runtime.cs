using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Universe.Dashboard.DAL.MultiProvider
{
    public class MySqlProvider4Runtime : IProvider4Runtime
    {
        public EF.Family Family => EF.Family.MySql;
        
        public void ValidateConnectionString(string connectionString)
        {
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(connectionString);
        }

        public string SetPooling(string connectionString, bool pooling)
        {
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(connectionString);
            b.Pooling = pooling;
            return b.ConnectionString;
        }

        public string SetConnectionTimeout(string connectionString, int connectionTimeout)
        {
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(connectionString);
            b.ConnectionTimeout = (uint) connectionTimeout;
            return b.ConnectionString;
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public void ApplyDbContextOptions(DbContextOptionsBuilder builder, string connectionString)
        {
            builder.UseMySQL(connectionString, options =>
            {
                options.MigrationsHistoryTable(DashboardContextOptionsFactory.MigrationsTableName);
            });
        }

        public string GetShortVersion(IDbConnection connection, int? commandTimeout = 20)
        {
            return connection.ExecuteScalar<string>("Select version();", commandTimeout: commandTimeout);
        }

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
        
        public void CreateMigrationHistoryTableIfAbsent(IDbConnection connection, string migrationsHistoryTable)
        {
            var sqlSelect = string.Format(SqlSelectHistoryTable_MySQL, migrationsHistoryTable);
            var existingHistoryTable = connection.ExecuteScalar<string>(sqlSelect);
            if (existingHistoryTable == null)
            {
                var sqlCreate = string.Format(SqlCreateHistoryTable_MySQL, migrationsHistoryTable);
                connection.Execute(sqlCreate);
            }
        }
    }
}