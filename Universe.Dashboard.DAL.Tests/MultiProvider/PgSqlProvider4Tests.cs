using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Universe.Dashboard.DAL.Tests.MultiProvider
{
    public class PgSqlProvider4Tests : IProvider4Tests
    {
        public List<string> GetServerConnectionStrings()
        {
            return MultiProviderExtensions.GetServerConnectionStrings("PGSQL_TEST_SERVER");
        }

        public string CreateDatabase(string serverConnectionString, string dbName)
        {
            NpgsqlConnectionStringBuilder b = new NpgsqlConnectionStringBuilder(serverConnectionString);
            Console.WriteLine($@"Creating PgSQL database ""{dbName}"" on server {b.Host}:{b.Port}");
            using (var con = new NpgsqlConnection(serverConnectionString))
            {
                con.Execute($@"Create Database ""{dbName}"";");
            }

            b.Database = dbName;
            return b.ConnectionString;
        }

        public void DropDatabase(string serverConnectionString, string dbName)
        {
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(serverConnectionString);
            Console.WriteLine($@"Deleting PgSQL database ""{dbName}"" on server {b.Server}:{b.Port}");
            using (var con = new NpgsqlConnection(serverConnectionString))
            {
                con.Execute($@"Drop Database ""{dbName}"";");
            }
        }

        public void ApplyDbContextOptions(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.ApplyPgSqlOptions(connectionString);
        }

        public void Migrate(DbContext db)
        {
            EFMigrations.Migrate_PgSQL(db, DashboardContextOptionsFactory.MigrationsTableName);
        }

        public string GetServerName(string connectionString)
        {
            NpgsqlConnectionStringBuilder b = new NpgsqlConnectionStringBuilder(connectionString);
            return $"PgSQL server {b.Host}:{b.Port}";
        }
        
        public string EnvVarName => DashboardContextOptions4PgSQL.CONNECTION_ENV_NAME;
    }
}