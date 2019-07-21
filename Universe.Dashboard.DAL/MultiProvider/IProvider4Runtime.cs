using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL.MultiProvider
{
    public interface IProvider4Runtime
    {
        EF.Family Family { get; }
        void ValidateConnectionString(string connectionString);
        IDbConnection CreateConnection(string connectionString);
        void ApplyDbContextOptions(DbContextOptionsBuilder builder, string connectionString);
        string GetShortVersion(IDbConnection connection);

        // Sqlite does nothing
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
        
        public static Exception WaitFor(this IProvider4Runtime provider, IDbConnection db, int timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Exception ret = null;
            do
            {
                try
                {
                    provider.GetShortVersion(db);
                    return null;
                }
                catch (Exception ex)
                {
                    ret = ex;
                    if (sw.ElapsedMilliseconds > timeout) return ret;
                    Thread.Sleep(200);
                }

            } while (sw.ElapsedMilliseconds < timeout);

            return ret;
        }
    }
}