using System.Collections.Generic;
using System.Linq;
using KernelManagementJam;
using Microsoft.AspNetCore.Mvc;
using Universe.Dashboard.Agent;

namespace ReactGraphLab.Controllers
{
    [Route("api/benchmark/disk")]
    public class DiskBenchmarkController
    {
        [HttpGet, Route("get-list")]
        public List<DriveDetails> GetList()
        {
            return MountsDataSource.Mounts.FilterForHuman().OrderBy(x => x.MountEntry.MountPath).ToList();
        }
    }
}