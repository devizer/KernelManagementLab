using System;
using System.Collections.Generic;
using System.Linq;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Microsoft.AspNetCore.Mvc;
using Universe.Dashboard.Agent;

namespace Universe.W3Top.Controllers
{
    [Route("api/[controller]")]
    public class ProcessListController
    {
        [HttpGet, HttpPost, Route("")]
        public List<AdvancedProcessStatPoint> GetProcesses()
        {
            var uptime = UptimeParser.ParseUptime();
            
            var snapshot = ProcessIoStat.GetProcesses();
            var ret = snapshot
                .Where(x => !x.IsZombie)
                .Select(x => new AdvancedProcessStatPoint(x) { Uptime = Math.Round(uptime.Value - x.StartAt,2)})
                .ToList();
            
            DebugDumper.Dump(ret, "AdvancedProcessStatPoint[].json", minify: false);
            return ret;

        }
    }
}