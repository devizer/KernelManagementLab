using System.Data;
using Dapper;
using SQLitePCL;

namespace Tests
{
    public class MySqlServerManager
    {
        private readonly IDbConnection Connection;

        public MySqlServerManager(IDbConnection connection)
        {
            Connection = connection;
        }

        public void CreateDatabase(string name, string charset = "utf8", string collation = "utf8_unicode_ci")
        {
            Connection.Execute($"Create Database `{name}` CHARACTER SET {charset} COLLATE {collation};");
        }

        public void DropDatabase(string name)
        {
            Connection.Execute($"Drop Database `{name}`;");
        }
    }
}