using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptions4Sqlite
    {
        public static DbContextOptions DesignTimeOptions =>
            new DbContextOptionsBuilder()
                .ApplySqliteOptions(SqliteDatabaseOptions.DbFullPath)
                .Options;

        public static DbContextOptionsBuilder ApplySqliteOptions(this DbContextOptionsBuilder optionsBuilder, string fullFileName)
        {
            optionsBuilder.UseSqlite($"DataSource={fullFileName}", options =>
            {
                options.MigrationsHistoryTable(DashboardContextOptionsFactory.MigrationsTableName);
                options.SuppressForeignKeyEnforcement(true);
            });

            return optionsBuilder;
        }
    }
}