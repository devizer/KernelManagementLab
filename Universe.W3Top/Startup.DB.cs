using System;
using Dapper;
using KernelManagementJam;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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
                        var exception = Providers4Runtime.WaitFor(provider, runtimeParameters.ConnectionString, 30000);
                        if (exception != null)
                            Console.WriteLine(
                                $"{runtimeParameters.Family} server is not ready. {exception.GetExceptionDigest()}");
                    }
                    
                    provider.Migrate(dashboardContext, runtimeParameters.ConnectionString);
/*

                    var historyRepository = dashboardContext.Database.GetService<IHistoryRepository>();
                    bool historyExists = historyRepository.Exists();
                    Console.WriteLine($"historyRepository.Exists() is {historyExists}");
                    if (!historyExists)
                    {
                        string createScript = historyRepository.GetCreateScript();
                        Console.WriteLine($"historyRepository.GetCreateScript() is {Environment.NewLine}{createScript}");
                        var conSetup = provider.CreateConnection(runtimeParameters.ConnectionString);
                        using (conSetup)
                        {

                            conSetup.Execute(createScript);
                        }
                    }
*/
                }
                
                
            }

            using (var dashboardContext = new DashboardContext())
            {
                var shortVersion = dashboardContext.Database.GetShortVersion();
                Console.WriteLine($"DB Server is ready. Its version is: {shortVersion}");
            }
        }
        
    }
}