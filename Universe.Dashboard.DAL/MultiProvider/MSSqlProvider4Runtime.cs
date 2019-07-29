using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL.MultiProvider
{
    public class MSSqlProvider4Runtime : IProvider4Runtime
    {
        public EF.Family Family => EF.Family.SqlServer;

        public void ValidateConnectionString(string connectionString)
        {
            SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(connectionString);
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public void ApplyDbContextOptions(DbContextOptionsBuilder builder, string connectionString)
        {
            builder.UseSqlServer(connectionString, options =>
            {
                options.MigrationsHistoryTable(DashboardContextOptionsFactory.MigrationsTableName);
            });
        }

        public string GetShortVersion(IDbConnection connection, int? commandTimeout)
        {
            int ver32Bit = connection.ExecuteScalar<int>("Select @@MICROSOFTVERSION", commandTimeout: commandTimeout);
            int v1 = ver32Bit >> 24;
            int v2 = ver32Bit >> 16 & 0xFF;
            int v3 = ver32Bit & 0xFFFF;
            var ver = new Version(v1, v2, v3);
            return ver.ToString();
        }

        public string SetPooling(string connectionString, bool pooling)
        {
            SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(connectionString);
            b.Pooling = pooling;
            return b.ConnectionString;
        }

        public string SetConnectionTimeout(string connectionString, int connectionTimeout)
        {
            SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(connectionString);
            b.ConnectTimeout = connectionTimeout;
            return b.ConnectionString;
        }

        public string GetServerName(string connectionString)
        {
            SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(connectionString);
            return $"MS SQL server {b.DataSource}";
        }

        public void CreateMigrationHistoryTableIfAbsent(IDbConnection connection, string migrationsHistoryTable)
        {
            // TODO: not yet necessary
        }
    }
}