using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Universe;
using Universe.Dashboard.Agent;

namespace ReactGraphLab
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            PidFileSupport.CreatePidFile();
            JitCrossInfo();
            DebugDumps();
            var webHost = CreateWebHostBuilder(args).Build();

            PreciseTimer.Services = webHost.Services;
            NetStatTimer.Process();
            DebugPreciseTimer();
            
            webHost.Run();
        }

        static void JitCrossInfo()
        {
            var info = CrossInfo.HumanReadableEnvironment(4);
            Console.WriteLine($"System Environment:{Environment.NewLine}{info}");
        }


        static void DebugPreciseTimer()
        {
            var name = "PreciseTimer::Debugger";
            Stopwatch sw = null;
            int counter = -1;
            PreciseTimer.AddListener(name,() =>
            {
                return;
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
            DebugDumper.Dump(snapshot, "SysBlocks.Snapshot.json");
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}