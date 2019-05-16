using System;
using System.IO;
using System.Threading;
using KernelManagementJam;
using NDesk.Options;
using Universe.Benchmark.DiskBench;

namespace Universe.DiskBench
{
    class Program
    {
        private static long FileSize = 4096 * 1024; // Kb
        private static int BlockSize = 4096;
        private static string Disk = ".";
        private static int RandomDuration = 30000;

        static int Main(string[] args)
        {


            bool nologo = false;
            bool help = false;
            var p = new OptionSet(StringComparer.InvariantCultureIgnoreCase)
            {
                {"d|Disk=", "Path ot disk. Default is current", v => Disk = v},
                {"s|Size=", "Working Size (Kb) default is 4096*1024", v => FileSize = int.Parse(v)},
                {"b|Block=", "Random access block size, default is 4096", v => BlockSize = int.Parse(v)},
                {"t|Time=", "Random access duration (milliseconds), default is 30000", v => RandomDuration = int.Parse(v)},
                {"h|?|Help", "Display this help", v => help = v != null},
                {"n|nologo", "Hide logo", v => nologo = v != null}
            };

            p.Parse(args);
            if (!nologo)
            {
                Console.WriteLine(@"Disk Benchmark tool is a sandbox of the KernelManagementJam for w3top app");
            }

            if (help)
            {
                p.WriteOptionDescriptions(Console.Out);
                return 0;
            }
            
            Disk = new DirectoryInfo(Disk).FullName;
            
            // JIT
            var tempSize = 32*1024;
            DiskBenchmark jit = new DiskBenchmark(Disk, tempSize, tempSize, 1);
            jit.Perform();
            // return 0;

            DiskBenchmark dbench = new DiskBenchmark(Disk, FileSize*1024L, BlockSize, stepDuration:RandomDuration);
            ManualResetEvent done = new ManualResetEvent(false);
            

            Action updateProgress = () =>
            {
                Console.Clear();
                Console.WriteLine($"Disk: {Disk}, Size: {Formatter.FormatBytes(FileSize*1024L)}, Block: {BlockSize} bytes");
                WriteProgress(dbench.Prorgess.Clone());
            };

            ThreadPool.QueueUserWorkItem(_ =>
            {
                do
                {
                    updateProgress();
                } while (!done.WaitOne(499));
            });
            
            ThreadPool.QueueUserWorkItem(_ => { 
                dbench.Perform();
                done.Set();
            });

            done.WaitOne();
            updateProgress();
            return 0;

        }

        static void WriteProgress(ProgressInfo progress)
        {
            foreach (var step in progress.Steps)
            {
                var s = $"[{step.State}]";
                if (step.PerCents.HasValue) s += " " + ((step.PerCents.Value * 100).ToString("##0.0")).PadLeft(5) + "%";
                if (step.Seconds.HasValue)
                    s += " " + new DateTime(0).Add(TimeSpan.FromSeconds(step.Seconds.Value)).ToString("HH:mm:ss");

                var b = step.Bytes == 0 || !step.Seconds.HasValue ? "" : Formatter.FormatBytes((long) (step.Bytes / step.Seconds.Value)) + "/s";
                if (b != "") s += " " + b.PadLeft(9);

                s += " " + step.Name;
                
                Console.WriteLine(s);
            }
        }
        
    }
}