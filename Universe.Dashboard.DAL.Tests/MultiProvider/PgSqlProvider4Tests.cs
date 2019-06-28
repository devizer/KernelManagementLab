using System;
using System.Collections.Generic;
using Dapper;
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
            Console.WriteLine($@"Creating Postres SQL database ""{dbName}"" on server {b.Host}:{b.Port}");
            using (MySqlConnection con = new MySqlConnection(serverConnectionString))
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
            using (MySqlConnection con = new MySqlConnection(serverConnectionString))
            {
                con.Execute($@"Drop Database ""{dbName}"";");
            }
        }

    }
}