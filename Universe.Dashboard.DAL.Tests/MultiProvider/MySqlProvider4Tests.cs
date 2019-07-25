using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;
using Universe.Dashboard.DAL.MultiProvider;

namespace Universe.Dashboard.DAL.Tests.MultiProvider
{
    public class MySqlProvider4Tests : IProvider4Tests
    {
        private const string SERVERS_PATTERN = "MYSQL_TEST_SERVER";
        public string EnvVarName => DashboardContextOptionsFactory.EnvNames.MySqlDb;
        public IProvider4Runtime Provider4Runtime => Providers4Runtime.MySql;

        public List<string> GetServerConnectionStrings()
        {
            return MultiProviderExtensions.GetServerConnectionStrings(SERVERS_PATTERN);
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
            using (MySqlConnection con = new MySqlConnection(serverConnectionString))
            {
                con.Execute($"Drop Database `{dbName}`;");
            }
        }

    }
}