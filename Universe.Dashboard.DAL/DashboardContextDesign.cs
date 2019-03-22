using System;
using System.IO;

namespace Universe.Dashboard.DAL
{
    public class DashboardContextDesign
    {
        public static string DbPath => _DbPath.Value; 
        
        private static Lazy<string> _DbPath = new Lazy<string>(() =>
        {
            var ret = GetDbFileName();
            if (!Directory.Exists(Path.GetDirectoryName(ret)))
                Directory.CreateDirectory((Path.GetDirectoryName(ret)));
            
            Console.WriteLine($"Universe.Dashboard.DAL DB: {ret}");

            return ret;
        });

        private static string GetDbFileName()
        {
            var isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var varName = isWin ? "APPDATA" : "HOME";
            var dir = Environment.GetEnvironmentVariable(varName);
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