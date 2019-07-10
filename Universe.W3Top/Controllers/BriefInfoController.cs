using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Universe.Dashboard.Agent;

namespace Universe.W3Top.Controllers
{
    [Route("api/[controller]")]
    public class BriefInfoController
    {
        [HttpGet, Route("")]
        public SystemBriefInfo Get()
        {
            var hostInfo = new
            {
                Hostname = Environment.MachineName,
                Os = CrossInfo.OsDisplayName,
                Processor = CrossInfo.ProcessorName,
                Memory = CrossInfo.TotalMemory == null
                    ? "n/a"
                    : string.Format("{0:n0} Mb", CrossInfo.TotalMemory / 1024),
            };

            SystemBriefInfo ret = new SystemBriefInfo()
            {
                System = hostInfo,
                InterfaceNames = NetDataSourceView.GetInterfaceNames(),
                BlockNames = BlockDiskDataSourceView.GetDiskOrVolNames(),
                NewVer = NewVersionDataSource.NewVersion,
            };

            return ret;
        }
    }
    
    public class SystemBriefInfo
    {
        public dynamic System;
        public List<string> InterfaceNames;
        public List<string> BlockNames;
        public JObject NewVer;
    }
}