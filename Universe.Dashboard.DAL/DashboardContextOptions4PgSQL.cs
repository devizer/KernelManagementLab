using System;
using KernelManagementJam;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptions4PgSQL
    {
        public static readonly string CONNECTION_ENV_NAME = "PGSQL_DATABASE";
        public static string ConnectionString => Environment.GetEnvironmentVariable(CONNECTION_ENV_NAME)?.Trim();
        
        public static DbContextOptions DesignTimeOptions =>
            new DbContextOptionsBuilder()
                .ApplyPgSqlOptions(ConnectionString)
                .Options;
        
        public static DbContextOptionsBuilder ApplyPgSqlOptions(this DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.MigrationsHistoryTable(DashboardContextOptionsFactory.MigrationsTableName);
            });

            return optionsBuilder;
        }
        
        public static void ValidateConnectionString()
        {
            try
            {
                NpgsqlConnectionStringBuilder b = new NpgsqlConnectionStringBuilder(ConnectionString);
                b.Timeout = 30;
                NpgsqlConnection con = new NpgsqlConnection(b.ConnectionString);
                // using(con) con.Open();
            }
            catch (Exception ex)
            {
                var msg = $"Invalid PgSQL Connection String [{ConnectionString}]. {ex.GetExceptionDigest()}";
                Console.WriteLine(msg);
                throw new ArgumentException(msg, ex);
            }
        }

    }
}