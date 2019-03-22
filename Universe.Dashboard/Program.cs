using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ReactGraphLab
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            DebugDumps();
            CreateWebHostBuilder(args).Build().Run();
        }

        private static void DebugDumps()
        {
            ProcMountsSandbox.DumpProcMounts();
            List<WithDeviceWithVolumes> snapshot = SysBlocksReader.GetSnapshot();
            DebugDumper.Dump(snapshot, "debug-dumps/SysBlocks.Snapshot.js");
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}