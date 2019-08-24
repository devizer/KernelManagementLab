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
using Universe.Dashboard.DAL.MultiProvider;
using Universe.Dashboard.DAL.Tests.MultiProvider;
using EF = Universe.Dashboard.DAL.MultiProvider.EF;

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
            var providers = new IProvider4Tests[] { new MySqlProvider4Tests(), new PgSqlProvider4Tests(), new SqlServerProvider4Tests(), };
            foreach (var provider4Tests in providers)
            {
                var serverConnectionStrings = provider4Tests.GetServerConnectionStrings();
                foreach (var serverConnectionString in serverConnectionStrings)
                {
                    counter++;
                    var dbName = string.Format(dbNameFormat, (char) (64 + counter));
                    var artifact = $"DB `{dbName}` on {provider4Tests.Provider4Runtime.GetServerName(serverConnectionString)}";

                    var dbConnectionString = provider4Tests.CreateDatabase(serverConnectionString, dbName);

                    Func<string, DashboardContext> newDbContext = delegate(string cs)
                    {
                        Environment.SetEnvironmentVariable("MYSQL_DATABASE", null);
                        Environment.SetEnvironmentVariable("PGSQL_DATABASE", null);
                        Environment.SetEnvironmentVariable("MSSQL_DATABASE", null);
                        Environment.SetEnvironmentVariable(provider4Tests.EnvVarName, dbConnectionString);
                        
                        var optionsBuilder = new DbContextOptionsBuilder();
                        provider4Tests.Provider4Runtime.ApplyDbContextOptions(optionsBuilder, cs);
                        return new DashboardContext(optionsBuilder.Options);
                    };

                    GlobalCleanUp.Enqueue($"Delete {artifact}", () => { provider4Tests.DropDatabase(serverConnectionString, dbName); });

                    var db = newDbContext(dbConnectionString);
                    GracefulFail($"Apply migrations for {artifact}", () => provider4Tests.Provider4Runtime.Migrate(db, dbConnectionString));
                    
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

        static void GracefulFail(string caption, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Fail: {caption}. {ex.GetExceptionDigest()}", ex);
            }
        }

        private static DbTestParameter CreateSqlLiteDbTestParameter()
        {
            var family = EF.Family.Sqlite;
            using (var db = CreateSqliteDbContext())
            {
                db.Database.Migrate();
                var shortVer = db.Database.GetShortVersion();
                return new DbTestParameter
                {
                    Family = family,
                    ShortVersion = shortVer,
                    GetDashboardContext = () =>
                    {
                        Environment.SetEnvironmentVariable(DashboardContextOptionsFactory.EnvNames.MySqlDb, null);
                        Environment.SetEnvironmentVariable(DashboardContextOptionsFactory.EnvNames.PgSqlDb, null);
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
            
            var dbContextOptionsBuilder = new DbContextOptionsBuilder();
            Providers4Runtime.Sqlite.ApplyDbContextOptions(dbContextOptionsBuilder, $"DataSource={testFile}");
            DashboardContext ret = new DashboardContext(dbContextOptionsBuilder.Options);
            return ret;
        }
    }
}
