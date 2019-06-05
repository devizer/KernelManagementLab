using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Universe.Dashboard.DAL;

namespace Tests
{
    public class DbEnv
    {
        public static DashboardContext CreateDbContext()
        {
            var runtimeFile = DashboardContextDefaultOptions.DbFullPath;
            var testFile = Path.Combine(Path.GetDirectoryName(runtimeFile), "nunit", Path.GetFileName(runtimeFile));

            var dir = Path.GetDirectoryName(testFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            
            var opts = new DbContextOptionsBuilder()
                .ApplyDashboardDbOptions(testFile)
                .Options;

            DashboardContext ret = new DashboardContext(opts);
            return ret;

        }
    }
}