using System;
using KernelManagementJam;
using Universe.Dashboard.DAL;
using Universe.Dashboard.DAL.MultiProvider;
using RelationalDatabaseFacadeExtensions = Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions;

namespace Universe.W3Top
{
    partial class Startup
    {
        private static void CreateOrUpgradeDb()
        {
            var runtimeParameters = DashboardContextOptionsFactory.RuntimeParameters;
            using (StopwatchLog.ToConsole($"Create/Upgrade {runtimeParameters.Family} DB Structure"))
            using (var dashboardContext = new DashboardContext())
            {
                var provider = DashboardContextOptionsFactory.Family.GetProvider();
                provider.ValidateConnectionString(runtimeParameters.ConnectionString);
                if (runtimeParameters.Family != EF.Family.Sqlite)
                {
                    using (StopwatchLog.ToConsole($"Check {runtimeParameters.Family} server health"))
                    {
                        using (var conHealth = provider.CreateConnection(runtimeParameters.ConnectionString))
                        {
                            var exception = Provider4Runtime.WaitFor(provider, conHealth, 30000);
                            if (exception != null)
                                Console.WriteLine($"{runtimeParameters.Family} server is not ready. {exception.GetExceptionDigest()}");
                        }
                    }

                    var conSetup = provider.CreateConnection(runtimeParameters.ConnectionString);
                    using (conSetup)
                    {
                        provider.CreateMigrationHistoryTableIfAbsent(conSetup, DashboardContextOptionsFactory.MigrationsTableName);
                    }
                }
                
                RelationalDatabaseFacadeExtensions.Migrate(dashboardContext.Database);
            }
        }
        
        [Obsolete]
        private static void CreateOrUpgradeDb_Legacy()
        {
            var runtimeParameters = DashboardContextOptionsFactory.RuntimeParameters;
            using (StopwatchLog.ToConsole($"Create/Upgrade DB Structure [{DashboardContextOptionsFactory.Family}]"))
            using (var dashboardContext = new DashboardContext())
            {
                var provider = DashboardContextOptionsFactory.Family.GetProvider();
                provider.ValidateConnectionString(runtimeParameters.ConnectionString);
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