using System;
using System.Collections.Generic;
using System.Linq;
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
                Os = HugeCrossInfo.OsDisplayName,
                Processor = HugeCrossInfo.ProcessorName,
                Memory = HugeCrossInfo.TotalMemory == null
                    ? "n/a"
                    : string.Format("{0:n0} Mb", HugeCrossInfo.TotalMemory / 1024),
            };

            SystemBriefInfo ret = new SystemBriefInfo()
            {
                System = hostInfo,
                InterfaceNames = NetDataSourceView.GetInterfaceNames(),
                BlockNames = GetOrderedBlockNames(),
                NewVer = NewVersionDataSource.NewVersion,
            };

            return ret;
        }


        private static List<string> GetOrderedBlockNames()
        {
            return BlockDevicesUI.GetOrderedBlockNames(BlockDiskDataSourceView.GetDiskOrVolNames());
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