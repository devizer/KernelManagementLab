using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Universe.Dashboard.DAL;
using EF = Universe.Dashboard.DAL.EF;

namespace Tests
{
    public class DbEnv
    {
        public static bool IsTravis => Environment.GetEnvironmentVariable("TRAVIS") == "true";


        public static List<DbParameter> TestParameters
        {
            get
            {
                List<DbParameter> ret = new List<DbParameter>
                {
                    new DbParameter
                    {
                        Family = EF.Family.Sqlite,
                        GetDashboardContext = () =>
                        {
                            Environment.SetEnvironmentVariable("MYSQL_DATABASE", null);
                            return CreateSqliteDbContext();
                        }
                    }
                };

                // ret.Clear();
                if (/*!IsTravis &&*/ MySqlTestEnv.NeedMySqlTests)
                {
                    EFMigrations.Migrate_MySQL(MySqlTestEnv.CreateMySQLDbContext(), DashboardContextOptionsFactory.MigrationsTableName);

                    ret.Add(new DbParameter
                    {
                        Family = EF.Family.MySql,
                        GetDashboardContext = () =>
                        {
                            Environment.SetEnvironmentVariable("MYSQL_DATABASE", MySqlTestEnv.TestMySqlConnection);
                            return MySqlTestEnv.CreateMySQLDbContext();
                        }
                    });
                }

                return ret;
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

    public class DbParameter
    {
        public EF.Family Family;
        public Func<DashboardContext> GetDashboardContext;
        public override string ToString()
        {
            return $"DB {Family.ToString()}";
        }
    }
}