using System;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public class DashboardContext : DbContext
    {
        
        public DbSet<DbInfo> DbInfo { get; set; }

        public DashboardContext() : base(DashboardContextOptions.DesignTimeOptions)
        {
            Console.WriteLine("DashboardContext()");
        }

        public DashboardContext(DbContextOptions<DashboardContext> options) : base(options)
        {
            Console.WriteLine("DashboardContext(DbContextOptions options)");
        }

    }
}
