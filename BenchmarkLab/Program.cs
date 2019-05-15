using System;
using System.Threading;
using KernelManagementJam;
using Universe.Benchmark.DiskBench;

namespace Universe.DiskBench
{
    class Program
    {
        private static readonly long DefaultFileSize = 10000L*1024*1024;

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("arg is path ot disk");
                return 1;
            }

            var folder = args[0];
            
            DiskBenchmark dbench = new DiskBenchmark(folder, DefaultFileSize, 4*1024, stepDuration:30000);
            ManualResetEvent done = new ManualResetEvent(false);

            Action updateProgress = () =>
            {
                Console.Clear();
                Console.WriteLine($"Folder: {folder}, Size: {Formatter.FormatBytes(DefaultFileSize)}");
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
                if (b != "") s += " " + b.PadLeft(8);

                s += " " + step.Name;
                
                Console.WriteLine(s);
            }
        }
        
        
    }
}