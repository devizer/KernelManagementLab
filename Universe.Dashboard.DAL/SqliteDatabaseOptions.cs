using System;
using System.IO;
using System.Threading;
using SQLitePCL;

namespace Universe.Dashboard.DAL
{
    public static class SqliteDatabaseOptions
    {
        public static string DbFullPath => _DbPath.Value;

        private static Lazy<string> _DbPath = new Lazy<string>(GetSmartyDbName, LazyThreadSafetyMode.ExecutionAndPublication);

        // All the implementation is exclusive
        private static string GetSmartyDbName()
        {
            var ret = GetDbFileName();
            var directoryName = Path.GetDirectoryName(ret);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            Console.WriteLine($"Universe.Dashboard.DAL SqLite DB: {ret}");
            GarbageCollector4Sqlite.CleanUpPrevVersions(ret);
            return ret;
        }

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
            var ver = typeof(DashboardContext).Assembly.GetName().Version.ToString().Replace(".",".");
            var relPath = new[] {".cache", "W3Top", $"history-{ver}.sqlite"};
            var fullPath = Path.Combine(dir2, string.Join(Path.DirectorySeparatorChar.ToString(), relPath));
            return fullPath;
        }

    }
}
