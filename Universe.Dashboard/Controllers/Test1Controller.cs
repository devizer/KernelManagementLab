using System.Collections.Generic;
using KernelManagementJam;
using Microsoft.AspNetCore.Mvc;

namespace ReactGraphLab.Controllers
{
    [Route("api/[controller]")]
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