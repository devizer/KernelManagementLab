using System;
using System.Collections.Generic;
using KernelManagementJam;
using Microsoft.AspNetCore.Mvc;

namespace Universe.W3Top.Controllers
{
    [Route("api/[controller]")]
    [Obsolete("wipe it off", error:false)]
    public class Test1Controller : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult BeCrazy()
        {
            List<WithDeviceWithVolumes> snapshot = SysBlocksReader.GetSnapshot();
            return Ok(snapshot);
        }
    }
}