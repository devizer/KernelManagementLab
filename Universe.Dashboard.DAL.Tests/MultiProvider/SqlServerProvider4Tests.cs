using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using MySql.Data.MySqlClient;
using Universe.Dashboard.DAL.MultiProvider;

namespace Universe.Dashboard.DAL.Tests.MultiProvider
{
    public class SqlServerProvider4Tests : IProvider4Tests
    {
        private const string SERVERS_PATTERN = "MSSQL_TEST_SERVER";
        public string EnvVarName => DashboardContextOptionsFactory.EnvNames.MSSqlDb;
        public IProvider4Runtime Provider4Runtime => Providers4Runtime.SqlServer;
        public List<string> GetServerConnectionStrings()
        {
            return MultiProviderExtensions.GetServerConnectionStrings(SERVERS_PATTERN);
        }
        
        public string CreateDatabase(string serverConnectionString, string dbName)
        {
            SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(serverConnectionString);
            var artifact = $"MS SQL Server database `{dbName}` on server {b.DataSource}";
            MultiProviderExtensions.RiskyAction($"Creating {artifact}", () =>
            {
                using (var con = new SqlConnection(serverConnectionString))
                {
                    con.Execute($"Create Database [{dbName}];");
                }
            });
            
            b.InitialCatalog = dbName;
            return b.ConnectionString;
        }
        
        public void DropDatabase(string serverConnectionString, string dbName)
        {
            using (SqlConnection con = new SqlConnection(serverConnectionString))
            {
                con.Execute($"Drop Database [{dbName}];");
            }
        }



    }
}