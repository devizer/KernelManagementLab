using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VisualizeFioTestMatrix
{
    class DataSourceReader
    {
        public static List<RawBenchmark> Read()
        {
            List<RawBenchmark> ret = new List<RawBenchmark>();
            var dirs1 = new DirectoryInfo("structured-fio-benchmark-results").GetDirectories();
            using(Log.Duration("Read raw structured-fio-benchmark-results"))
            foreach (var dir1 in dirs1)
            {
                var summaryFiles = dir1.GetFiles("*.summary");
                Console.WriteLine($"{summaryFiles.Length} benchmarks in [{dir1.Name}]");
                ParallelOptions po = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
                Parallel.ForEach(summaryFiles, po, summaryFile =>
                {
                    RawBenchmark rawBenchmark = Parse(File.ReadAllText(summaryFile.FullName));
                    lock (ret) ret.Add(rawBenchmark);
                });
            }

            return ret;
        }

        static RawBenchmark Parse(string content)
        {
            RawBenchmark ret = new RawBenchmark();
            var lines = content.Split('\r', '\n').Where(x => x.Trim().Length > 0).ToArray();
            foreach (var line in lines)
            {
                int p = line.IndexOf(':');
                if (p > 0 && p < line.Length - 1)
                {
                    string key = line.Substring(0, p).ToLower();
                    string value = line.Substring(p + 1).Trim();
                    if (key == "benchmark.engine") ret.Engine = value;
                    if (key == "host.machine") ret.Arch = value;
                    if (key == "benchmark.exit.code") ret.ExitCode = Int32.Parse(value);
                    if (key == "fio.raw") ret.FioRaw = value;
                    if (key == "host.image") ret.Image = value;
                    if (key == "host.os") ret.OsAndVersion = value;
                    if (key == "host.glibc") ret.GLibCVersion = value;
                    if (key == "host.container") ret.ContainerName = value;
                }
            }

            return ret;
        }
    }
}
