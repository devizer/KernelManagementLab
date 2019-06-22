using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptionsFactory
    {
        public static string MigrationsTableName = "W3TopUpgradeHistory";

        public static DbContextOptions Create()
        {
            return (Family == EF.Family.MySql)
                ? DashboardContextOptions4MySQL.DesignTimeOptions
                : DashboardContextOptions4Sqlite.DesignTimeOptions;
        }

        public static EF.Family Family
        {
            get
            {
                var cs = DashboardContextOptions4MySQL.ConnectionString;
                return string.IsNullOrEmpty(cs) ? EF.Family.Sqlite : EF.Family.MySql;  
            }
        }
    }
}