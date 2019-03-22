using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public class DashboardContext : DbContext
    {
        
        public DbSet<DbInfo> Info { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"DataSource={DashboardContextDesign.DbPath}");
        }

    }
}