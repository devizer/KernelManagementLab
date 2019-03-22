using System;
using System.IO;
using System.Threading;
using SQLitePCL;

namespace Universe.Dashboard.DAL
{
    public static class DashboardContextDefaultOptions
    {
        public static string DbPath => _DbPath.Value;

        // All the implementation is exclusive
        private const LazyThreadSafetyMode ExclusiveMode = LazyThreadSafetyMode.ExecutionAndPublication;

        private static Lazy<string> _DbPath = new Lazy<string>(() =>
        {
            var ret = GetDbFileName();
            var directoryName = Path.GetDirectoryName(ret);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            Console.WriteLine($"Universe.Dashboard.DAL DB: {ret}");
            DashboardContextGarbageCollector.CleanUpPrevVersions(ret);
            
            return ret;
        }, ExclusiveMode);


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
            var ver = typeof(DashboardContext).Assembly.GetName().Version.ToString();
            var relPath = new[] {".cache", "Web-Dashboard", $"history-{ver}.sqlite"};
            var fullPath = Path.Combine(dir2, string.Join(Path.DirectorySeparatorChar.ToString(), relPath));
            return fullPath;
        }

    }
}