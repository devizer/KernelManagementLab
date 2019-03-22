using System.Diagnostics;
using System.IO;
using System.Linq;
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
            var location = DashboardContextDefaultOptions.DbFullPath;
            var size = new FileInfo(location).Length;
            return new DbHealpthStatus()
            {
                Location = location,
                Size = size,
                Latency = msec,
            };
        }

        public class DbHealpthStatus
        {
            public double Latency { get; set; }
            public string Location { get; set; }
            public long Size { get; set; }
        }

        
    }
}