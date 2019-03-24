using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Universe.Dashboard.Agent;

namespace ReactGraphLab
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            DebugDumps();
            var webHost = CreateWebHostBuilder(args).Build();
            
            using (var scope = webHost.Services.CreateScope())
            {
                var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
                DebugPreciseTimer(loggerFactory);
                NetStatTimer.Process(loggerFactory);
            }
            
            webHost.Run();
        }

        static void DebugPreciseTimer(ILoggerFactory loggerFactory)
        {
            var name = "PreciseTimer::Debugger";
            var logger = loggerFactory.CreateLogger(name);
            Stopwatch sw = null;
            int counter = -1;
            PreciseTimer.AddListener(name, logger, () =>
            {
                counter = (counter + 1) % 4;
                if (counter == 0) throw new Exception("Fail simulation");
                long startAt = 0;
                if (sw == null)
                {
                    sw = Stopwatch.StartNew();
                    startAt = sw.ElapsedTicks;
                }
                else
                {
                    long ticks = sw.ElapsedTicks;
                    double msec = (ticks - startAt) *1000d / Stopwatch.Frequency;
                    Console.WriteLine($"PreciseTimer: {msec:n2} msec");
                    Thread.Sleep(new Random().Next(100,777));
                }
            });
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