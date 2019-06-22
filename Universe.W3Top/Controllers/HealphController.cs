using System.Diagnostics;
using System.IO;
using System.Linq;
using KernelManagementJam;
using Microsoft.AspNetCore.Mvc;
using Universe.Dashboard.DAL;

namespace ReactGraphLab.Controllers
{
    
    [Route("api/[controller]")]
    public class HealthController
    {
        private DashboardContext _Db;

        public HealthController(DashboardContext db)
        {
            _Db = db;
        }

        [HttpGet("[action]")]
        public string Ping()
        {
            return "Pong";
        }

        [HttpGet("[action]")]
        public DbHealpthStatus PingDb()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var dummy = _Db.DbInfo.Select(x => x.Id).FirstOrDefault(x => false);
            var msec = sw.ElapsedTicks / (double) Stopwatch.Frequency;
            var location = SqliteDatabaseOptions.DbFullPath;
            var size = new FileInfo(location).Length;
            var sizeInfo = Formatter.FormatBytes(size);
            return new DbHealpthStatus()
            {
                Location = location,
                Size = size,
                Latency = msec,
                HumanSize = sizeInfo,
            };
        }

        public class DbHealpthStatus
        {
            public double Latency { get; set; }
            public string Location { get; set; }
            public long Size { get; set; }
            public string HumanSize { get; set; }
        }

        
    }
}