using System;
using System.Net.Http;
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
            IProvider4Runtime provider = DashboardContextOptionsFactory.Family.GetProvider();
            // Prevent start if connection string is malformed.
            provider.ValidateConnectionString(runtimeParameters.ConnectionString);
            
            using (StopwatchLog.ToConsole($"Create/Upgrade {runtimeParameters.Family} DB Structure"))
            using (var dashboardContext = new DashboardContext())
            {
                // sqlite is always ready and another existing db-file is never used
                if (runtimeParameters.Family != EF.Family.Sqlite)
                {
                    using (StopwatchLog.ToConsole($"Check {runtimeParameters.Family} server health"))
                    {
                        var exception = Providers4Runtime.WaitFor(provider, runtimeParameters.ConnectionString, 30000);
                        if (exception != null)
                            Console.WriteLine(
                                $"{runtimeParameters.Family} server is not ready. {exception.GetExceptionDigest()}");
                    }
                    
                    provider.Migrate(dashboardContext, runtimeParameters.ConnectionString);
                }
            }

            using (var dashboardContext = new DashboardContext())
            {
                RelationalDatabaseFacadeExtensions.Migrate(dashboardContext.Database);
            }

            using (var dashboardContext = new DashboardContext())
            {
                var shortVersion = dashboardContext.Database.GetShortVersion();
                Console.WriteLine($"DB Server is ready. Its version is: {shortVersion}");
            }
        }
    }
}