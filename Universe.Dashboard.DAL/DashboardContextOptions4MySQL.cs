using System;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptions4MySQL
    {
        public static DbContextOptions DesignTimeOptions =>
            new DbContextOptionsBuilder()
                .ApplyMySqlOptions(ConnectionString)
                .Options;

        public static DbContextOptionsBuilder ApplyMySqlOptions(this DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseMySQL(connectionString, options =>
            {
                options.MigrationsHistoryTable("UpgradeHistory");
            });

            return optionsBuilder;
        }

        public static string ConnectionString
        {
            get
            {
                var defaultConnectionString = "Server=localhost;Database=w3top;Port=3306;Uid=admin;Pwd=admin;Connect Timeout=5;Pooling=false;";
                // return defaultConnectionString;
                var ret = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
                if (string.IsNullOrEmpty(ret)) ret = defaultConnectionString;
                return ret;
            }
        }
    }
    
    
}