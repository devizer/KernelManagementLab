using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL.MultiProvider
{
    public class SqliteProvider4Runtime : IProvider4Runtime
    {
        public EF.Family Family => EF.Family.Sqlite;
        
        public void ValidateConnectionString(string connectionString)
        {
            SqliteConnectionStringBuilder b = new SqliteConnectionStringBuilder(connectionString);
        }

        public string SetPooling(string connectionString, bool pooling)
        {
            return connectionString;
        }

        public string SetConnectionTimeout(string connectionString, int connectionTimeout)
        {
            return connectionString;
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqliteConnection(connectionString);
        }

        public void ApplyDbContextOptions(DbContextOptionsBuilder builder, string connectionString)
        {
            builder.UseSqlite(connectionString, options =>
            {
                options.MigrationsHistoryTable(DashboardContextOptionsFactory.MigrationsTableName);
            });
        }

        public string GetShortVersion(IDbConnection connection, int? commandTimeout = 20)
        {
            return connection.ExecuteScalar<string>("Select sqlite_version();", commandTimeout: commandTimeout);
        }

        public void CreateMigrationHistoryTableIfAbsent(IDbConnection connection, string migrationsHistoryTable)
        {
            // nothing to do - existing DB is never used for sqlite db
        }

    }
}