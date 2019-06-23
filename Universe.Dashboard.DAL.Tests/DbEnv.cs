using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using Universe.Dashboard.DAL;
using EF = Universe.Dashboard.DAL.EF;

namespace Tests
{
    public class DbEnv
    {
        public const string TestMySqlConnection = "Server=localhost;Database=w3top;Port=3306;Uid=w3top;Pwd=w3top;Connect Timeout=5;Pooling=false;";
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
                Environment.SetEnvironmentVariable("MYSQL_DATABASE", TestMySqlConnection);
                if (!IsTravis && CreateMySQLDbContext() != null)
                {
                    EFMigrations.Migrate_MySQL(CreateMySQLDbContext(), DashboardContextOptionsFactory.MigrationsTableName);

                    ret.Add(new DbParameter
                    {
                        Family = EF.Family.MySql,
                        GetDashboardContext = () =>
                        {
                            Environment.SetEnvironmentVariable("MYSQL_DATABASE", TestMySqlConnection);
                            return CreateMySQLDbContext();
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

        public static DashboardContext CreateMySQLDbContext()
        {
            var cs = DashboardContextOptions4MySQL.ConnectionString;
            if (string.IsNullOrEmpty(cs)) return null;
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(cs);
            b.Database = b.Database + "_tests";
            
            var options = new DbContextOptionsBuilder()
                .ApplyMySqlOptions(b.ConnectionString)
                .Options;
            
            return new DashboardContext(options);
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