using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.Common.Exceptions;
using MySql.Data.MySqlClient;
using Universe.Dashboard.DAL;

namespace Tests
{
    class MySqlTestEnv
    {
        // privileged connection string TO SERVER
        public static readonly string MYSQL_TEST_SERVER_ENV_NAME = "MYSQL_TEST_CONNECTION";
        public static string AdminConnectionString => Environment.GetEnvironmentVariable(MYSQL_TEST_SERVER_ENV_NAME);
        public static bool NeedMySqlTests => !string.IsNullOrEmpty(AdminConnectionString);
        public static string TestMySqlConnection => _TestMySqlConnection.Value;
        public static string DbName = $"W3Top_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_ffff}";


        private static Lazy<string> _TestMySqlConnection = new Lazy<string>(() =>
        {
            if (!NeedMySqlTests) return null;
            var adminConnectionString = AdminConnectionString;
            var dbName = DbName;
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(adminConnectionString);
            Console.WriteLine($"Creating database {dbName} on server {b.Server} port {b.Port}");
            using (var con = new MySqlConnection(adminConnectionString))
            {
                MySqlServerManager man = new MySqlServerManager(con);
                man.CreateDatabase(dbName);
                b.Database = dbName;
                AppDomain.CurrentDomain.DomainUnload += delegate
                {
                    Console.WriteLine("AppDomain.CurrentDomain.DomainUnload");
                };
                AppDomain.CurrentDomain.ProcessExit += delegate
                {
                    string file = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "ProcessExit.log");
                    File.AppendAllText(file, DateTime.Now.ToString() + Environment.NewLine);
                    
                    Console.WriteLine("AppDomain.CurrentDomain.ProcessExit");
                    Console.WriteLine($"Deleting DB {dbName} on server {b.Server} port {b.Port}");
                    using (var conToDelete = new MySqlConnection(adminConnectionString))
                    {
                        MySqlServerManager man2 = new MySqlServerManager(conToDelete);
                        man2.DropDatabase(dbName);
                    }
                };
                return b.ConnectionString;
            }
        });
        
        public const string TestMySqlConnection_Legacy = "Server=localhost;Database=w3top;Port=3306;Uid=w3top;Pwd=w3top;Connect Timeout=5;Pooling=false;";
        public static DashboardContext CreateMySQLDbContext()
        {
            if (!NeedMySqlTests) throw new InvalidLoggerException($"MySQL Tests are not configured. Env var {MYSQL_TEST_SERVER_ENV_NAME} is not defined");
            var cs = TestMySqlConnection;
            
            var options = new DbContextOptionsBuilder()
                .ApplyMySqlOptions(cs)
                .Options;
            
            return new DashboardContext(options);
        }
        
    }
}