using System;
using KernelManagementJam;
using Universe.Dashboard.DAL;
using RelationalDatabaseFacadeExtensions = Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions;

namespace ReactGraphLab
{
    partial class Startup
    {
        private static void CreateOrUpgradeDb()
        {
            using (StopwatchLog.ToConsole($"Create/Upgrade DB Structure [{DashboardContextOptionsFactory.Family}]"))
            using (var dashboardContext = new DashboardContext())
            {
                switch (DashboardContextOptionsFactory.Family)
                {
                    case EF.Family.PgSql:
                        DashboardContextOptions4PgSQL.ValidateConnectionString();
                        using (StopwatchLog.ToConsole($"Check postgres health"))
                        {
                            var exception = EFHealth.WaitFor(dashboardContext, 30000);
                            if (exception != null)
                                Console.WriteLine($"postgres is not ready. {exception.GetExceptionDigest()}");
                        }

                        EFMigrations.Migrate_PgSQL(dashboardContext, DashboardContextOptionsFactory.MigrationsTableName);
                        break;

                    case EF.Family.MySql:
                        DashboardContextOptions4MySQL.ValidateConnectionString();
                        using (StopwatchLog.ToConsole($"Check MySQL Server health"))
                        {
                            var exception = EFHealth.WaitFor(dashboardContext, 30000);
                            if (exception != null)
                                Console.WriteLine($"MySQL Server is not ready. {exception.GetExceptionDigest()}");
                        }

                        EFMigrations.Migrate_MySQL(dashboardContext, DashboardContextOptionsFactory.MigrationsTableName);
                        break;

                    case EF.Family.Sqlite:
                        RelationalDatabaseFacadeExtensions.Migrate(dashboardContext.Database);
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported DB provider family {DashboardContextOptionsFactory.Family}");
                }
            }
        }
    }
}