using System;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptions4MySQL
    {
        public static string MigrationHistoryTable => "UpgradeHistory";

        public static DbContextOptions DesignTimeOptions =>
            new DbContextOptionsBuilder()
                .ApplyDashboardDbOptions(ConnectionString)
                .Options;

        public static DbContextOptionsBuilder ApplyDashboardDbOptions(this DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseMySQL(connectionString, options =>
            {
                options.MigrationsHistoryTable(MigrationHistoryTable);
            });

            return optionsBuilder;
        }

        public static void ValidateConnectionString()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(ConnectionString);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid MySQL Connection String", ex);
            }
        }

        public static string ConnectionString
        {
            get
            {
                var defaultConnectionString = "Server=localhost;Database=w3top_b1;Port=3306;Uid=admin;Pwd=admin;Connect Timeout=5;Pooling=false;";
                // return defaultConnectionString;
                var ret = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
                if (string.IsNullOrEmpty(ret)) ret = defaultConnectionString;
                return ret;
            }
        }
    }
    
    
}
