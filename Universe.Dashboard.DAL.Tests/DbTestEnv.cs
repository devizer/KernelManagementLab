using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using KernelManagementJam;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Tests;
using Universe.Dashboard.DAL.Tests.MultiProvider;

namespace Universe.Dashboard.DAL.Tests
{
    public class DbTestEnv
    {
        public static bool IsTravis => Environment.GetEnvironmentVariable("TRAVIS") == "true";
        
        static Lazy<List<DbTestParameter>> _TestParameters = new Lazy<List<DbTestParameter>>(GetTestParameters, LazyThreadSafetyMode.ExecutionAndPublication);

        public static List<DbTestParameter> TestParameters => _TestParameters.Value;

        static List<DbTestParameter> GetTestParameters()
        {
            List<DbTestParameter> ret = new List<DbTestParameter> {CreateSqlLiteDbParameter()};
            var dbName = $"W3Top_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_ffff}";

            {
                MySqlProvider4Tests provider = new MySqlProvider4Tests();
                var serverConnectionStrings = provider.GetServerConnectionStrings();
                foreach (var serverConnectionString in serverConnectionStrings)
                {
                    MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(serverConnectionString);
                    var artifact = $"MySQL `{dbName}` on server {b.Server}:{b.Port}";

                    Func<string, DashboardContext> newDbContext = delegate(string cs)
                    {
                        var options = new DbContextOptionsBuilder().ApplyMySqlOptions(cs).Options;
                        return new DashboardContext(options);
                    };

                    GlobalCleanUp.Enqueue($"Cleanup {artifact}",
                        () => { provider.DropDatabase(serverConnectionString, dbName); });

                    var dbConnectionString = provider.CreateDatabase(serverConnectionString, dbName);
                    EFMigrations.Migrate_MySQL(newDbContext(dbConnectionString),
                        DashboardContextOptionsFactory.MigrationsTableName);

                    ret.Add(new DbTestParameter()
                    {
                        Family = EF.Family.MySql,
                        GetDashboardContext = () => newDbContext(dbConnectionString)
                    });
                }
            }

            return ret;
        }

        private static DbTestParameter CreateSqlLiteDbParameter()
        {
            return new DbTestParameter
            {
                Family = EF.Family.Sqlite,
                GetDashboardContext = () =>
                {
                    Environment.SetEnvironmentVariable("MYSQL_DATABASE", null);
                    return CreateSqliteDbContext();
                }
            };
        }


        public static DashboardContext CreateSqliteDbContext()
        {
            var runtimeFile = SqliteDatabaseOptions.DbFullPath;
            var testFile = Path.Combine(Path.GetDirectoryName(runtimeFile), "nunit", Path.GetFileName(runtimeFile));

            var dir = Path.GetDirectoryName(testFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            
            var opts = new DbContextOptionsBuilder()
                .ApplySqliteOptions(testFile)
                .Options;

            DashboardContext ret = new DashboardContext(opts);
            return ret;
        }
    }

    public class DbTestParameter
    {
        public EF.Family Family;
        public Func<DashboardContext> GetDashboardContext;
        public override string ToString()
        {
            return $"{Family.ToString()}-DB";
        }
    }
}
