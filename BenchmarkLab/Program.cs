using System;
using System.Collections.Generic;
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
        const DataGeneratorFlavour DefaultFlavour = DataGeneratorFlavour.Random;
        static DataGeneratorFlavour Flavour = DefaultFlavour;

        static int Main(string[] args)
        {

            // Check_Readonly_O_Direct();

            bool nologo = false;
            bool help = false;
            bool version = false;
            var ver = Assembly.GetEntryAssembly().GetName().Version;
            var p = new OptionSet(StringComparer.InvariantCultureIgnoreCase)
            {
                {"p|Path=", "Path ot disk. Default is current", v => Disk = v},
                {"s|Size=", "Working Set size (Kb) default is 4096*1024", v => FileSize = ValidateAndParse("Working Set Size", v)},
                {"b|Block=", "Random access block size, default is 4096", v => BlockSize = ValidateAndParse("Random Access Block Size", v)},
                {"t|Time=", "Random access duration (milliseconds), default is 30000", v => RandomDuration = ValidateAndParse("Random Access Duration", v)},
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
            
            var theLogo = $@"Disk Benchmark [v{ver}] tool is a console spinoff of the KernelManagementJam for w3top app";
            
            if (!nologo)
            {
                Console.WriteLine(theLogo);
            }

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

            
            Disk = new DirectoryInfo(Disk).FullName;
            if (!Directory.Exists(Disk))
                throw new Exception($"Target path {Disk} does not exists");
            
            // JIT
            var tempSize = 128*1024;
            var hasWritePermission = DiskBenchmarkChecks.HasWritePermission(Disk);
            IDiskBenchmark jit;
            if (hasWritePermission)
                jit = new DiskBenchmark(Disk, tempSize, Flavour, tempSize, 1);
            else
                jit = new ReadonlyDiskBenchmark(new DiskBenchmarkOptions()
                {
                    WorkFolder = Disk, 
                    StepDuration = 1, 
                    ThreadsNumber = 2,
                    DisableODirect = false,
                    WorkingSetSize = 128 * 1024,
                    RandomAccessBlockSize = 128 * 1024,
                }) { IsJit = true};
            
            jit.Perform();
            // return 0;

            IDiskBenchmark dbench;
            if (hasWritePermission)
                dbench = new DiskBenchmark(
                    Disk,
                    FileSize * 1024L,
                    Flavour,
                    BlockSize,
                    RandomDuration,
                    DisableODirect);
            else
            dbench = new ReadonlyDiskBenchmark(new DiskBenchmarkOptions()
            {
                WorkFolder = Disk,
                WorkingSetSize = FileSize * 1024L,
                StepDuration = RandomDuration,
                ThreadsNumber = 16,
                DisableODirect = false,
                RandomAccessBlockSize = BlockSize
            });
            
            ManualResetEvent done = new ManualResetEvent(false);

            Action updateProgress = () =>
            {
                WriteProgress(dbench.Progress.Clone());
            };

            bool hasDotsBuffer = false;
            var prevCompletedStep = dbench.Progress.Clone().LastCompleted;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                do
                {
                    if (Console.IsOutputRedirected)
                    {
                        var nextCompletedStep = dbench.Progress.Clone().LastCompleted;
                        if (nextCompletedStep != null && nextCompletedStep?.Name != prevCompletedStep?.Name)
                        {
                            prevCompletedStep = nextCompletedStep; 
                            if (hasDotsBuffer) Console.WriteLine();
                            
                            hasDotsBuffer = false;
                            Console.WriteLine(FormatStepAsHumanString(nextCompletedStep));
                        }
                    }

                    if (!Console.IsOutputRedirected)
                        updateProgress();
                    else
                    {
                        Console.Write(".");
                        hasDotsBuffer = true;
                    }
                } while (!done.WaitOne(499));

            });

            ThreadPool.QueueUserWorkItem(_ => { 
                dbench.Perform();
                done.Set();
            });

            done.WaitOne();
            if (hasDotsBuffer && Console.IsOutputRedirected) Console.WriteLine();
            updateProgress();
            return 0;

        }

        private static bool Check_Readonly_O_Direct()
        {
            const string fff = "/usr/lib/x86_64-linux-gnu/libLLVM-3.9.so.1";
            Console.WriteLine($"Checking O_Direct for {fff}");
            bool has = DiskBenchmarkChecks.IsO_DirectSupported_Readonly("/usr/lib/x86_64-linux-gnu/libLLVM-3.9.so.1", 16384);
            Console.WriteLine($"O_Direct for {fff}: [{has}]");
            return has;
        }

        static void WriteProgress(ProgressInfo progress)
        {

            StringBuilder buf = new StringBuilder();
            buf.AppendLine($"Disk: {Disk}, WorkingSet: {Formatter.FormatBytes(FileSize*1024L)}, Flavour: {Flavour}, Block: {BlockSize} bytes");
            foreach (var step in progress.Steps)
            {
                var s = FormatStepAsHumanString(step);

                buf.AppendLine(s);
            }
            
            if (!Console.IsOutputRedirected) Console.Clear();
            Console.Write(buf);
        }

        private static string FormatStepAsHumanString(ProgressStep step)
        {
            var s = $"[{step.State}]".PadRight(12);
            if (step.PerCents.HasValue) s += " " + ((step.PerCents.Value * 100).ToString("##0.0")).PadLeft(5) + "%";
            if (step.Seconds.HasValue)
                s += " " + new DateTime(0).Add(TimeSpan.FromSeconds(step.Seconds.Value)).ToString("HH:mm:ss");

            var b = step.Bytes == 0 || !step.Seconds.HasValue
                ? ""
                : Formatter.FormatBytes((long) (step.Bytes / step.Seconds.Value)) + "/s";
            
            if (b != "") s += " " + b.PadLeft(9);

            var cpuUsage = "";
            if (step.CpuUsage.HasValue && step.Seconds.HasValue && step.Seconds > 0)
            {
                Func<double, string> formatPerCents = seconds => (100d * seconds / step.Seconds.Value).ToString("####0.0").PadLeft(7); 
                var usage = step.CpuUsage.Value;
                double pcUser = 100d * usage.UserUsage.TotalSeconds / step.Seconds.Value;
                double pcKernel = 100d * usage.KernelUsage.TotalSeconds / step.Seconds.Value;
                cpuUsage = $"  {pcUser:0.0} + {pcKernel:#0.0}%".PadRight(13);
                // cpuUsage = $"{formatPerCents(usage.UserUsage.TotalSeconds)}% [user] + {formatPerCents(usage.KernelUsage.TotalSeconds)}% [kernel]";

            }

            s += cpuUsage;
            s += " " + step.Name;
            return s;
        }

        static DataGeneratorFlavour ParseFlavour(string raw)
        {
            switch (raw.ToLower())
            {
                case "42":
                case "forty-two":
                case "fortytwo":
                    return DataGeneratorFlavour.FortyTwo;
                
                case "random":
                    return DataGeneratorFlavour.Random;

                case "stable-random":
                    return DataGeneratorFlavour.StableRandom;
                
                case "lorem-ipsum":
                    return DataGeneratorFlavour.LoremIpsum;

                case "stable-lorem-ipsum":
                    return DataGeneratorFlavour.StableLoremIpsum;

                case "il":
                case "il-code":
                case "code":
                    return DataGeneratorFlavour.ILCode;
                
                default:
                    return DefaultFlavour;

            }
        }


        static void FunnyException()
        {
            string file = Assembly.GetEntryAssembly().Location;
            int n = 0;
            List<object> holder = new List<object>();
            Console.Write("Opened: ");
            while (true)
            {
                holder.Add(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 8));
                n++;
                if (n % 1000 == 0) Console.Write ($"{(n/1000)}K ");
            }
        }

        static int ValidateAndParse(string parameterName, string raw)
        {
            if (int.TryParse(raw, out var ret))
                return ret;
            
            throw new ArgumentException($"Incorrect parameter '{parameterName}'. The value '{raw}' is not valid integer.");
        }
        
    }
}