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
                // sqlite is always ready and another existing db-file is never used
                if (runtimeParameters.Family != EF.Family.Sqlite)
                {
                    using (StopwatchLog.ToConsole($"Check {runtimeParameters.Family} server health"))
                    {
                        using (var conHealth = provider.CreateConnection(runtimeParameters.ConnectionString))
                        {
                            var exception = Providers4Runtime.WaitFor(provider, conHealth, 30000);
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
        
    }
}