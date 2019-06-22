using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptionsFactory
    {
        public static string MigrationsTableName = "W3TopUpgradeHistory";

        public static DbContextOptions Create()
        {
            var mySqlConnectionString = DashboardContextOptions4MySQL.ConnectionString;
            if (!string.IsNullOrEmpty(mySqlConnectionString))
                return DashboardContextOptions4MySQL.DesignTimeOptions;

            return DashboardContextOptions4Sqlite.DesignTimeOptions;
        }

        public static EF.Family Family
        {
            get
            {
                var mySqlConnectionString = DashboardContextOptions4MySQL.ConnectionString;
                if (!string.IsNullOrEmpty(mySqlConnectionString))
                    return EF.Family.MySql;

                return EF.Family.Sqlite;
            }
        }
    }
}