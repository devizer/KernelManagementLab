using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Dapper;
using KernelManagementJam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Universe.Dashboard.DAL.MultiProvider
{
    public interface IProvider4Runtime
    {
        EF.Family Family { get; }
        void ValidateConnectionString(string connectionString);
        IDbConnection CreateConnection(string connectionString);
        void ApplyDbContextOptions(DbContextOptionsBuilder builder, string connectionString);
        string GetShortVersion(IDbConnection connection, int? commandTimeout = 20);

        string SetPooling(string connectionString, bool pooling);
        string SetConnectionTimeout(string connectionString, int connectionTimeout);

        string GetServerName(string connectionString);

        
        // for compatiblity, but sqlite provider do nothing
        void CreateMigrationHistoryTableIfAbsent(IDbConnection connection, string migrationsHistoryTable);
    }
    

    public static class Providers4Runtime
    {
        public static readonly MySqlProvider4Runtime MySql = new MySqlProvider4Runtime();
        public static readonly PgSqlProvider4Runtime PgSql = new PgSqlProvider4Runtime();
        public static readonly SqliteProvider4Runtime Sqlite = new SqliteProvider4Runtime();

        public static IProvider4Runtime GetProvider(this EF.Family family)
        {
            if (family == EF.Family.Sqlite) return Sqlite;
            // if (family == EF.Family.SqlServer) return SqlServer;
            if (family == EF.Family.MySql) return MySql;
            if (family == EF.Family.PgSql) return PgSql;
            throw new ArgumentException($"Unknown provider family {family}", nameof(family));
        }

        public static void Migrate(this IProvider4Runtime provider, DbContext dashboardContext, string connectionString)
        {
            var historyRepository = dashboardContext.Database.GetService<IHistoryRepository>();

            // string createScript = historyRepository.GetCreateIfNotExistsScript(); // doesnt work for mysql 5.1 - 5.5
            // hack
            string createScript = historyRepository.GetCreateScript();
            Console.WriteLine($"historyRepository.GetCreateScript() is {Environment.NewLine}{createScript}");
            using (var con = provider.CreateConnection(connectionString))
            {
                try
                {
                    con.Execute(createScript);
                }
                catch
                {
                    // hack, because historyRepository.Exists() sometimes returns false
                    // in case of fail the call to Database.Migrate() will throw the same.
                }
            }

            dashboardContext.Database.Migrate();
        }

        public static Exception WaitFor(this IProvider4Runtime provider, string connectionString, int timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var tunedConnectionString = provider.SetConnectionTimeout(provider.SetPooling(connectionString, false), 5);
            Exception ret = null;
            // string artifact = $"{provider.Family} server {provider.get}"
            var debugMsgHeader = $"Check health of {provider.GetServerName(tunedConnectionString)}: ";
            StringBuilder debugProgress = new StringBuilder(debugMsgHeader);
            string GetMSec() => ((double) sw.ElapsedTicks / Stopwatch.Frequency).ToString("n2");
                
            do
            {
                using (var con = provider.CreateConnection(tunedConnectionString))
                {
                    try
                    {
                        provider.GetShortVersion(con, 5);
                        Console.WriteLine($"{debugProgress} OK in {GetMSec()}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        ret = ex;
                        if (sw.ElapsedMilliseconds > timeout) return ret;
                        debugProgress.Append($" {GetMSec() :(,}");
                        Console.WriteLine($"{debugProgress}");
                        Thread.Sleep(200);
                    }
                }

            } while (sw.ElapsedMilliseconds < timeout);

            if (ret != null)
                Console.WriteLine($"{debugMsgHeader} {ret.GetExceptionDigest()}{Environment.NewLine}{ret}");

            return ret;
        }
    }
}