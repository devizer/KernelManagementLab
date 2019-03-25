using System;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public class DashboardContext : DbContext
    {

        public DbSet<DbInfo> DbInfo { get; set; }
        public DbSet<HistoryCopy> HistoryCopy { get; set; }

        public DashboardContext() : base(DashboardContextOptions.DesignTimeOptions)
        {
            // Console.WriteLine("Warning! DashboardContext() (default constructor is for design-time only)");
        }

        public DashboardContext(DbContextOptions<DashboardContext> options) : base(options)
        {
        }

    }
}
