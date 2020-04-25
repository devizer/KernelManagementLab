using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Universe.Dashboard.Agent;

namespace Universe.W3Top
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("--version", StringComparison.OrdinalIgnoreCase))
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                Console.WriteLine(entryAssembly.GetName().Version);
                return;
            }

            Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
            PidFileSupport.CreatePidFile();
            JitCrossInfo();
            CheckCompliance();
            var webHost = CreateWebHostBuilder(args).Build();
            
            try
            {
                ICollection<string> addresses = webHost.ServerFeatures.Get<IServerAddressesFeature>().Addresses;
                IpConfig.AddAddresses(addresses);
                Console.WriteLine($"{addresses.Count} listening address(es): {string.Join(", ", addresses)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Oops. Unable to obtain actual addresses for http[s]-server. {ex.GetExceptionDigest()}");
            }


            PreciseTimer.Services = webHost.Services;
            NetStatTimer.Process();
            BlockDiskTimer.Process();
            MemorySummaryTimer.Process();
            DebugPreciseTimer();
            
            webHost.Run();
        }

        static void JitCrossInfo()
        {
            var info = HugeCrossInfo.HumanReadableEnvironment(4);
            Console.WriteLine($"System Environment:{Environment.NewLine}{info}");
        }

        [Conditional(("Never"))]
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

        private static void CheckCompliance()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
/*
                Stopwatch sw = Stopwatch.StartNew();
*/

                StopwatchLog.SafeToConsole(
                    "ProcMountsParser(/proc/mounts) successfully pre-jitted",
                    "ProcMountsParser(/proc/mounts) fails. Mounts page may not work properly.",
                    () => { ProcMountsSandbox.DumpProcMounts(); });

                StopwatchLog.SafeToConsole(
                    "LinuxMemorySummary.TryParse(/proc/meminfo) successfully pre-jitted",
                    "LinuxMemorySummary.TryParse(/proc/meminfo) fails. Memory usage page may not work properly.",
                    () =>
                    {
                        if (!LinuxMemorySummary.TryParse(out var info))
                            throw new NotSupportedException("LinuxMemorySummary.TryParse is not supported");
                    });

                StopwatchLog.SafeToConsole(
                    "SysBlocksReader(/sys/block) successfully pre-jitted",
                    "SysBlocksReader(/sys/block) fails. Live chart of the disk activity on may not work properly",
                    () =>
                    {
                        List<WithDeviceWithVolumes> snapshot = SysBlocksReader.GetSnapshot();
                        DebugDumper.Dump(snapshot, "SysBlocks.Snapshot.json");
                    });

/*
                try
                {
                    ProcMountsSandbox.DumpProcMounts();
                    Console.WriteLine($"ProcMountsParser(/proc/mounts) successfully pre-jitted in {sw.ElapsedMilliseconds} milliseconds");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Warning!  " + ex.GetExceptionDigest() + Environment.NewLine + ex);
                }

                try
                {
                    List<WithDeviceWithVolumes> snapshot = SysBlocksReader.GetSnapshot();
                    DebugDumper.Dump(snapshot, "SysBlocks.Snapshot.json");
                    Console.WriteLine($"SysBlocksReader(/sys/block) successfully pre-jitted in {sw.ElapsedMilliseconds} milliseconds");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Warning! SysBlocksReader(/sys/block) fails. live chart of the disk activity on may not work properly. " + ex.GetExceptionDigest() + Environment.NewLine + ex);
                }
*/
            });
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}