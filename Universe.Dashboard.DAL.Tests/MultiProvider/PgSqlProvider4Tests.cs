using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;
using Universe.Dashboard.DAL.MultiProvider;

namespace Universe.Dashboard.DAL.Tests.MultiProvider
{
    public class PgSqlProvider4Tests : IProvider4Tests
    {
        private const string SERVERS_PATTERN = "PGSQL_TEST_SERVER";
        public string EnvVarName => DashboardContextOptionsFactory.EnvNames.PgSqlDb;
        public IProvider4Runtime Provider4Runtime => Providers4Runtime.PgSql;

        public List<string> GetServerConnectionStrings()
        {
            return MultiProviderExtensions.GetServerConnectionStrings(SERVERS_PATTERN);
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
            using (var con = new NpgsqlConnection(serverConnectionString))
            {
                con.Execute($@"Drop Database ""{dbName}"";");
            }
        }

    }
}