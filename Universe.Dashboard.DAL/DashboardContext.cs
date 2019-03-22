using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public class DashboardContext : DbContext
    {
        public DbSet<Info> Info { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"DataSource={GetDbFileName()}");
        }

        public static string GetDbFileName()
        {
            var isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var varName = isWin ? "APPDATA" : "HOME";
            var dir = Environment.ExpandEnvironmentVariables(varName);
            if (string.IsNullOrEmpty(dir))
            {
                dir = Path.DirectorySeparatorChar + "tmp";
            }
            
            var dir2 = new DirectoryInfo(dir).FullName;
            var relPath = new[] {".local", "WebDashboard", "history.sqlite"};
            var fullPath = Path.Combine(dir2, string.Join(Path.DirectorySeparatorChar.ToString(), relPath));
            return fullPath;
        }
    }
}