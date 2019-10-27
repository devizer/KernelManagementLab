using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using KernelManagementJam;
using Microsoft.EntityFrameworkCore;
using Universe.Dashboard.DAL.MultiProvider;
using Universe.Dashboard.DAL.Tests.MultiProvider;
using EF = Universe.Dashboard.DAL.MultiProvider.EF;

namespace Universe.Dashboard.DAL.Tests
{
    public class DbTestEnv
    {
        public static bool IsTravis => Environment.GetEnvironmentVariable("TRAVIS") == "true";
        
        static Lazy<List<TestDatabase>> _TestDatabases = new Lazy<List<TestDatabase>>(GetTestDatabases, LazyThreadSafetyMode.ExecutionAndPublication);

        public static List<TestDatabase> TestDatabases => _TestDatabases.Value;

        static List<TestDatabase> GetTestDatabases()
        {
            List<TestDatabase> ret = new List<TestDatabase> {CreateSqlLiteDbTestParameter()};
            
            var dbNameFormat = $"W3Top_{{0}}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_ffff}";
            var providers = new IProvider4Tests[] { new MySqlProvider4Tests(), new PgSqlProvider4Tests(), new SqlServerProvider4Tests(), };
            var providerCounter = 0;
            List<ErrorDetails> errors = new List<ErrorDetails>();
            foreach (var provider4Tests in providers)
            {
                providerCounter++;
                var serverConnectionStrings = provider4Tests.GetServerConnectionStrings();
                int versionCounter = 0;
                foreach (var serverConnectionString in serverConnectionStrings)
                {
                    versionCounter++;
                    var dbName = string.Format(dbNameFormat, $"{(char) (64 + providerCounter)}{(char) (48 + versionCounter)}");
                    try
                    {
                        string shortVer;
                        using (var con = provider4Tests.Provider4Runtime.CreateConnection(serverConnectionString))
                            shortVer = provider4Tests.Provider4Runtime.GetShortVersion(con);

                        var artifact =
                            $"DB `{dbName}` on {provider4Tests.Provider4Runtime.GetServerName(serverConnectionString)} ver {shortVer}";

                        var dbConnectionString = provider4Tests.CreateDatabase(serverConnectionString, dbName);


                        Func<string, DashboardContext> newDbContext = delegate(string cs)
                        {
                            foreach (var pro in providers)
                                Environment.SetEnvironmentVariable(pro.EnvVarName, null);

                            Environment.SetEnvironmentVariable(provider4Tests.EnvVarName, dbConnectionString);

                            var optionsBuilder = new DbContextOptionsBuilder();
                            provider4Tests.Provider4Runtime.ApplyDbContextOptions(optionsBuilder, cs);
                            return new DashboardContext(optionsBuilder.Options);
                        };

                        if (provider4Tests.Provider4Runtime.Family == EF.Family.MySql)
                        {
                            GlobalCleanUp.Enqueue($"Dump {artifact}",
                                () => { MySqlDumper.Dump(dbConnectionString, $"bin/Databases/MySQL-{shortVer}.sql"); });
                        }

                        if (provider4Tests.Provider4Runtime.Family == EF.Family.PgSql)
                        {
                            GlobalCleanUp.Enqueue($"Dump {artifact}",
                                () => { PgSqlDumper.Dump(dbConnectionString, $"bin/Databases/PgSQL-{shortVer}.sql"); });
                        }

                        GlobalCleanUp.Enqueue($"Delete {artifact}",
                            () => { provider4Tests.DropDatabase(serverConnectionString, dbName); });

                        var db = newDbContext(dbConnectionString);
                        GracefulFail($"Apply migrations for {artifact}",
                            () => provider4Tests.Provider4Runtime.Migrate(db, dbConnectionString));


                        ret.Add(new TestDatabase()
                        {
                            Family = db.Database.GetFamily(),
                            ShortVersion = shortVer,
                            GetDashboardContext = () => newDbContext(dbConnectionString)
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new ErrorDetails()
                        {
                            Exception = ex,
                            ConnectionString = serverConnectionString,
                            Family = provider4Tests.Provider4Runtime.Family, 
                        });
                    }
                }
            }

            if (errors.Count > 0)
            {
                var getFormatted = errors.Select((x, i) =>
                    $" #{i}: {x.Family} [{x.ConnectionString}]: {x.Exception.GetExceptionDigest()}");

                var msg =
                    $"Total {errors.Count} database(s) are not accessible{Environment.NewLine}{string.Join(Environment.NewLine, getFormatted)}";
                
                throw new AggregateException(msg, errors.Select(x => x.Exception).ToArray());
            }

            return ret;
        }
        
        class ErrorDetails
        {
            public EF.Family Family;
            public string ConnectionString;
            public Exception Exception;
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

        private static TestDatabase CreateSqlLiteDbTestParameter()
        {
            var family = EF.Family.Sqlite;
            using (var db = CreateSqliteDbContext())
            {
                db.Database.Migrate();
                var shortVer = db.Database.GetShortVersion();
                return new TestDatabase
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
