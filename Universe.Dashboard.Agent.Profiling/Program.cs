using System;
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
            var p = Process.Start("pseudo-root-fs.7z.exe", "-y");
            p.WaitForExit();
            Console.WriteLine("Done!");

            var webHost = CreateWebHostBuilder(args).Build();
            PreciseTimer.Services = webHost.Services;
            BlockDiskTimer.Process();
            RunBlockStat();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();


        static void RunBlockStat()
        {
            while (true)
            {
                var blockStatStorage = BlockDiskDataSource.Instance.By_1_Seconds;
                var blockStatView = BlockDiskDataSourceView.AsViewModel(blockStatStorage);
                Thread.Sleep(1000);
                Console.Write("." + blockStatStorage.Count);
            }
            
        }
    }
}