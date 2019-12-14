using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Universe.Dashboard.Agent.Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var p = Process.Start("pseudo-root-fs.7z.exe", "-y");
                p.WaitForExit();
                Console.WriteLine("Pseudo Root FS on Windows extracted!");
            }

            RunBlockStat();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();


        static void RunBlockStat()
        {
            var webHost = CreateWebHostBuilder(new string[0]).Build();
            PreciseTimer.Services = webHost.Services;
            BlockDiskTimer.Process();

            var atStart = CpuUsage.CpuUsage.GetByThread().Value;
            int total = 0;
            while (true)
            {
                
                Thread.Sleep(1000);
                List<BlockDiskDataSourcePoint> blockStatStorage = 
                    new List<BlockDiskDataSourcePoint>(BlockDiskDataSource.Instance.By_1_Seconds);

                for (int i = 0; i < 100; i++)
                {
                    // var blockStatView = BlockDiskDataSourceView.AsViewModel(blockStatStorage);
                    total++;
                }

                var next = CpuUsage.CpuUsage.GetByThread().Value;
                var cpuUsage = CpuUsage.CpuUsage.Substruct(next, atStart);
                Console.WriteLine($"{blockStatStorage.Count}: {total}, {cpuUsage}");

                // if (blockStatStorage.Count >= 20 && Debugger.IsAttached) Debugger.Break();
            }
            
        }
    }
}