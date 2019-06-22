using System;
using Dapper;
using MySql.Data.MySqlClient;

namespace Universe.Dashboard.DAL.MySQL
{
    class Program
    {
        static void Main(string[] args)
        {
            var ver = GetMySQLVer(DashboardContextOptions4MySQL.ConnectionString);
            Console.WriteLine($"MYSQL VER: {ver}");
            // return;
            DashboardContext db = new DashboardContext();
            db.Database.EnsureCreated();
        }
        
        private static string GetMySQLVer(string cs)
        {
            MySqlConnection con = new MySqlConnection(cs);
            con.Open();
            var ver = con.ExecuteScalar<string>("Select Version();");
            return ver;
        }

    }
    

}