using System;
using System.Collections.Generic;
using Dapper;
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
            Console.WriteLine($"Creating MySQL database `{dbName}` on server {b.Server}:{b.Port}");
            using (MySqlConnection con = new MySqlConnection(serverConnectionString))
            {
                string charset = "utf8";
                string collation = "utf8_unicode_ci";
                con.Execute($"Create Database `{dbName}` CHARACTER SET {charset} COLLATE {collation};");
            }

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
    }
}