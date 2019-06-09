using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextOptions
    {
        public static DbContextOptions DesignTimeOptions =>
            new DbContextOptionsBuilder()
                .ApplyDashboardDbOptions(DashboardContextDefaultOptions.DbFullPath)
                .Options;

        public static DbContextOptionsBuilder ApplyDashboardDbOptions(this DbContextOptionsBuilder optionsBuilder, string fullFileName)
        {
            optionsBuilder.UseSqlite($"DataSource={fullFileName}", options =>
            {
                options.MigrationsHistoryTable("UpgradeHistory");
                options.SuppressForeignKeyEnforcement(true);
            });

            return optionsBuilder;
        }
    }
}