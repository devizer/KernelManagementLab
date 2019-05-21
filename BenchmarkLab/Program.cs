using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.Benchmarks;
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
        private static bool DisableODirect = false;
        const DataFlavour DefaultFlavour = DataFlavour.Random;
        static DataFlavour Flavour = DefaultFlavour;

        static int Main(string[] args)
        {
            bool nologo = false;
            bool help = false;
            bool version = false;
            var ver = Assembly.GetEntryAssembly().GetName().Version;
            var p = new OptionSet(StringComparer.InvariantCultureIgnoreCase)
            {
                {"p|Path=", "Path ot disk. Default is current", v => Disk = v},
                {"s|Size=", "Working Size (Kb) default is 4096*1024", v => FileSize = int.Parse(v)},
                {"b|Block=", "Random access block size, default is 4096", v => BlockSize = int.Parse(v)},
                {"t|Time=", "Random access duration (milliseconds), default is 30000", v => RandomDuration = int.Parse(v)},
                {"f|Flavour=", "Data flavour as random|42|lorem-ipsum|code, default is random", v => Flavour = ParseFlavour(v)},
                {"d|Disable-O-DIRECT", "Disable O_DIRECT, default is auto-detect", v => DisableODirect = !string.IsNullOrEmpty(v)},
                {"v|version", "Display version", v => version = v != null},
                {"h|?|Help", "Display this help", v => help = v != null},
                {"n|nologo", "Hide logo", v => nologo = v != null}
            };

            p.Parse(args);
            
            if (version)
            {
                Console.WriteLine(ver);
                return 0;
            }
            
            var theLogo = $@"Disk Benchmark [v{ver}] tool is a sandbox of the KernelManagementJam for w3top app";
            
            if (help)
            {
                p.WriteOptionDescriptions(Console.Out);
                Console.WriteLine(@"
If disk/volume supports compression it is important to specify a flavour of the data (-f=VALUE or --flavour=VALUE):
 - 42 (maximum compression ~99.9%)
 - lorem-ipsum (high compression ~80%)
 - code (popular, compression is ~50%)
 - random (compression impossible)");
                
                return 0;
            }

            if (!nologo)
            {
                Console.WriteLine(theLogo);
            }

            
            Disk = new DirectoryInfo(Disk).FullName;
            
            // JIT
            var tempSize = 128*1024;
            DiskBenchmark jit = new DiskBenchmark(Disk, tempSize, Flavour, tempSize, 1);
            jit.Perform();
            // return 0;

            DiskBenchmark dbench = new DiskBenchmark(
                Disk, 
                FileSize*1024L, 
                Flavour,
                BlockSize, 
                RandomDuration, 
                DisableODirect);
            
            ManualResetEvent done = new ManualResetEvent(false);
            

            Action updateProgress = () =>
            {
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

            StringBuilder buf = new StringBuilder();
            buf.AppendLine($"Disk: {Disk}, Size: {Formatter.FormatBytes(FileSize*1024L)}, Flavour: {Flavour}, Block: {BlockSize} bytes");
            foreach (var step in progress.Steps)
            {
                var s = $"[{step.State}]".PadRight(12);
                if (step.PerCents.HasValue) s += " " + ((step.PerCents.Value * 100).ToString("##0.0")).PadLeft(5) + "%";
                if (step.Seconds.HasValue)
                    s += " " + new DateTime(0).Add(TimeSpan.FromSeconds(step.Seconds.Value)).ToString("HH:mm:ss");

                var b = step.Bytes == 0 || !step.Seconds.HasValue ? "" : Formatter.FormatBytes((long) (step.Bytes / step.Seconds.Value)) + "/s";
                if (b != "") s += " " + b.PadLeft(9);

                s += " " + step.Name;

                buf.AppendLine(s);
            }
            
            Console.Clear();
            Console.Write(buf);
        }

        static DataFlavour ParseFlavour(string raw)
        {
            switch (raw.ToLower())
            {
                case "42":
                case "forty-two":
                case "fortytwo":
                    return DataFlavour.FortyTwo;
                
                case "random":
                    return DataFlavour.Random;

                case "stable-random":
                    return DataFlavour.StableRandom;
                
                case "lorem-ipsum":
                    return DataFlavour.LoremIpsum;

                case "stable-lorem-ipsum":
                    return DataFlavour.StableLoremIpsum;

                case "il":
                case "il-code":
                case "code":
                    return DataFlavour.ILCode;
                
                default:
                    return DefaultFlavour;

                    
            }
        }
        
    }
}