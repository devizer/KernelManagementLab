using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptionsFactory
    {
        public static string MigrationsTableName = "W3Top_MigrationsHistory";

        public static DbContextOptions Create()
        {
            var f = Family;
            if (f == EF.Family.MySql) return DashboardContextOptions4MySQL.DesignTimeOptions;
            if (f == EF.Family.PgSql) return DashboardContextOptions4PgSQL.DesignTimeOptions;
            return DashboardContextOptions4Sqlite.DesignTimeOptions;
        }

        public static EF.Family Family
        {
            get
            {
                if (!string.IsNullOrEmpty(DashboardContextOptions4MySQL.ConnectionString))
                    return EF.Family.MySql;
                
                if (!string.IsNullOrEmpty(DashboardContextOptions4PgSQL.ConnectionString))
                    return EF.Family.PgSql;

                return EF.Family.Sqlite;  
            }
        }
    }
}