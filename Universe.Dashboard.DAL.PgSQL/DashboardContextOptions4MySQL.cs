using System;
using KernelManagementJam;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptions4MySQL
    {
        public static readonly string CONNECTION_ENV_NAME = "MYSQL_DATABASE";

        public static DbContextOptions DesignTimeOptions =>
            new DbContextOptionsBuilder()
                .ApplyMySqlOptions(ConnectionString)
                .Options;

        public static DbContextOptionsBuilder ApplyMySqlOptions(this DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseMySQL(connectionString, options =>
            {
                options.MigrationsHistoryTable(DashboardContextOptionsFactory.MigrationsTableName);
            });

            return optionsBuilder;
        }

        public static void ValidateConnectionString()
        {
            try
            {
                MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(ConnectionString);
                b.ConnectionTimeout = 1;
                MySqlConnection con = new MySqlConnection(b.ConnectionString);
                using(con) con.Open();
            }
            catch (Exception ex)
            {
                var msg = $"Invalid MySQL Connection String [{ConnectionString}]. {ex.GetExceptionDigest()}";
                Console.WriteLine(msg);
                throw new ArgumentException(msg, ex);
            }
        }

        public static string ConnectionString
        {
            get
            {
                // var defaultConnectionString = "Server=localhost;Database=w3top;Port=3306;Uid=admin;Pwd=admin;Connect Timeout=5;Pooling=false;";
                // return defaultConnectionString;
                var ret = Environment.GetEnvironmentVariable(CONNECTION_ENV_NAME);
                // if (string.IsNullOrEmpty(ret)) ret = defaultConnectionString;
                return ret?.Trim();
            }
        }
    }
    
    
}