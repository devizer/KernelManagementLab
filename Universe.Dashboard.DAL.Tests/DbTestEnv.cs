using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using KernelManagementJam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using MySql.Data.MySqlClient;
using Npgsql;
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
            List<DbTestParameter> ret = new List<DbTestParameter> {CreateSqlLiteDbTestParameter()};
            var dbNameFormat = $"W3Top_{{0}}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_ffff}";
            int counter = 0;
            var providers = new IProvider4Tests[] { new MySqlProvider4Tests(), new PgSqlProvider4Tests()};
            foreach (var provider in providers)
            {
                var serverConnectionStrings = provider.GetServerConnectionStrings();
                foreach (var serverConnectionString in serverConnectionStrings)
                {
                    counter++;
                    var dbName = string.Format(dbNameFormat, (char) (64 + counter));
                    var artifact = $"DB `{dbName}` on {provider.GetServerName(serverConnectionString)}";

                    var dbConnectionString = provider.CreateDatabase(serverConnectionString, dbName);

                    Func<string, DashboardContext> newDbContext = delegate(string cs)
                    {
                        Environment.SetEnvironmentVariable("MYSQL_DATABASE", null);
                        Environment.SetEnvironmentVariable("PGSQL_DATABASE", null);
                        Environment.SetEnvironmentVariable(provider.EnvVarName, dbConnectionString);
                        
                        var optionsBuilder = new DbContextOptionsBuilder();
                        provider.ApplyDbContextOptions(optionsBuilder, cs);
                        return new DashboardContext(optionsBuilder.Options);
                    };

                    GlobalCleanUp.Enqueue($"Delete {artifact}", () => { provider.DropDatabase(serverConnectionString, dbName); });

                    var db = newDbContext(dbConnectionString);
                    provider.Migrate(db);
                    var shortVer = db.Database.GetShortVersion();
                    ret.Add(new DbTestParameter()
                    {
                        Family = db.Database.GetFamily(),
                        ShortVersion = shortVer,
                        GetDashboardContext = () => newDbContext(dbConnectionString)
                    });
                }
            }

            return ret;
        }

        private static DbTestParameter CreateSqlLiteDbTestParameter()
        {
            var family = EF.Family.Sqlite;
            using (var db = CreateSqliteDbContext())
            {
                var shortVer = db.Database.GetShortVersion();
                return new DbTestParameter
                {
                    Family = family,
                    ShortVersion = shortVer,
                    GetDashboardContext = () =>
                    {
                        Environment.SetEnvironmentVariable(DashboardContextOptions4MySQL.CONNECTION_ENV_NAME, null);
                        Environment.SetEnvironmentVariable(DashboardContextOptions4PgSQL.CONNECTION_ENV_NAME, null);
                        return CreateSqliteDbContext();
                    }
                };
            }

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
}
