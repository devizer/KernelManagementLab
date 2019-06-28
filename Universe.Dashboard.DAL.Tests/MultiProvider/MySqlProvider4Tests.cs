using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Universe.Dashboard.DAL.Tests.MultiProvider
{
    public class MySqlProvider4Tests : IProvider4Tests
    {
        public List<string> GetServerConnectionStrings()
        {
            return MultiProviderExtensions.GetServerConnectionStrings("MYSQL_TEST_SERVER");
        }

        public string CreateDatabase(string serverConnectionString, string dbName)
        {
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(serverConnectionString);
            var artifact = $"MySQL database `{dbName}` on server {b.Server}:{b.Port}";
            MultiProviderExtensions.RiskyAction($"Creating {artifact}", () =>
            {
                using (MySqlConnection con = new MySqlConnection(serverConnectionString))
                {
                    string charset = "utf8";
                    string collation = "utf8_unicode_ci";
                    con.Execute($"Create Database `{dbName}` CHARACTER SET {charset} COLLATE {collation};");
                }
            });
            
            b.Database = dbName;
            return b.ConnectionString;
        }

        public void DropDatabase(string serverConnectionString, string dbName)
        {
            // MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(serverConnectionString);
            // Console.WriteLine($"Deleting MySQL database `{dbName}` on server {b.Server}:{b.Port}");
            using (MySqlConnection con = new MySqlConnection(serverConnectionString))
            {
                con.Execute($"Drop Database `{dbName}`;");
            }
        }
        
        public void ApplyDbContextOptions(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.ApplyMySqlOptions(connectionString);
        }

        public void Migrate(DbContext db)
        {
            EFMigrations.Migrate_MySQL(db, DashboardContextOptionsFactory.MigrationsTableName);
        }

        public string GetServerName(string connectionString)
        {
            var b = new MySqlConnectionStringBuilder(connectionString);
            return $"MySQL server {b.Server}:{b.Port}";
        }

        public string EnvVarName => DashboardContextOptions4MySQL.CONNECTION_ENV_NAME;
    }
}