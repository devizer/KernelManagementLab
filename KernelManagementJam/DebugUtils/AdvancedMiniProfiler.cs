using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Universe.CpuUsage;

namespace KernelManagementJam.DebugUtils
{
    public class AdvancedMiniProfilerKeyPath
    {
        private readonly Lazy<int> HashCode;
        public string[] Path { get; }

        public AdvancedMiniProfilerKeyPath()
        {
            HashCode = new Lazy<int>(() =>
            {
                if (Path == null) return 0;
                int ret = 0;
                unchecked
                {
                    foreach (var p in Path)
                        ret = ret * 397 ^ p.GetHashCode();
                }

                return ret;

            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public AdvancedMiniProfilerKeyPath(params string[] path) : this()
        {
            Path = path;
        }

        public AdvancedMiniProfilerKeyPath Child(string childName)
        {
            return new AdvancedMiniProfilerKeyPath(Path.Concat(new[] {childName}).ToArray());
        }

        public override string ToString()
        {
            // const string arrow = " \x27a1 ";
            const string arrow = " \x2192 ";
            return string.Join(arrow, Path ?? new string[0]);
        }

        protected bool Equals(AdvancedMiniProfilerKeyPath other)
        {
            var len = Path.Length;
            var lenOther = other.Path.Length;
            if (len != lenOther) return false;
            for(int i=0; i<len; i++)
                if (!Path[i].Equals(other.Path[i], StringComparison.InvariantCultureIgnoreCase))
                    return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AdvancedMiniProfilerKeyPath) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Value;
        }
    }

    public class AdvancedMiniProfilerMetrics
    {
        public double Duration { get; set; }
        public CpuUsage? CpuUsage { get; set; }
        public long Count { get; set; }
    }

    public class AdvancedMiniProfilerReport
    {
        private Dictionary<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics> Report = new Dictionary<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics>();
        private Dictionary<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics> FirstCall = new Dictionary<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics>();
        readonly object Sync = new object();
        public static readonly AdvancedMiniProfilerReport Instance = new AdvancedMiniProfilerReport();
        long _Timestamp = 0;

        public long Timestamp
        {
            get
            {
                lock (Sync) return _Timestamp;
            }
        }

        public void Add(AdvancedMiniProfilerKeyPath path, AdvancedMiniProfilerMetrics metrics)
        {
            lock (Sync)
            {
                // using (AdvancedMiniProfiler.Step("Advanced Profiler", "Increment Metrics"))
                {
                    Add_Impl(path, metrics);
                }
            }
        }

        private void Add_Impl(AdvancedMiniProfilerKeyPath path, AdvancedMiniProfilerMetrics metrics)
        {
            if (FirstCall.ContainsKey(path))
            {
                AdvancedMiniProfilerMetrics prev = Report.GetOrAdd(path,
                    key => new AdvancedMiniProfilerMetrics());
                prev.Duration += metrics.Duration;
                if (prev.CpuUsage.HasValue || metrics.CpuUsage.HasValue)
                    prev.CpuUsage = CpuUsage.Add(
                        prev.CpuUsage.GetValueOrDefault(),
                        metrics.CpuUsage.GetValueOrDefault());

                prev.Count += 1;
            }
            else
                FirstCall[path] = metrics;

            _Timestamp++;
        }

        public ConsoleTable AsConsoleTable()
        {
            KeyValuePair<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics>[] reportCopy, firstCallCopy; 
            lock (Sync)
            {
                reportCopy = this.Report.ToArray();
                firstCallCopy = this.FirstCall.ToArray();
            }

            reportCopy = reportCopy.OrderBy(x => x.Key.ToString()).ToArray();
            ConsoleTable ret = new ConsoleTable("Path", "-N", 
                "-Duration", "-CPU (%)", "-CPU (\x3bcs)", "-User", "-Kernel",
                "-1st Duration", "-1st CPU (%)", "-1st CPU (\x3bcs)", "-1st User", "-1st Kernel");

            var zeroMetrics = new AdvancedMiniProfilerMetrics();
            foreach (var pair in reportCopy)
            {
                var path = pair.Key;
                var total = pair.Value;
                var first = firstCallCopy.FirstOrDefault(x => x.Key.Equals(path)).Value ?? zeroMetrics;
                ret.AddRow(path.ToString(), total.Count,
                    (1000d * total.Duration / total.Count).ToString("n3"),
                    // total
                    100d * total.CpuUsage.GetValueOrDefault().TotalMicroSeconds / 1000000d / total.Duration,
                    (total.CpuUsage.GetValueOrDefault().TotalMicroSeconds / 1000d / total.Count).ToString("n3"),
                    (1000d * total.CpuUsage.GetValueOrDefault().UserUsage.TotalSeconds / total.Count).ToString("n3"),
                    (1000d * total.CpuUsage.GetValueOrDefault().KernelUsage.TotalSeconds / total.Count).ToString("n3"),
                    // first
                    (1000d * first.Duration).ToString("n3") ,
                    100d * first.CpuUsage.GetValueOrDefault().TotalMicroSeconds / 1000000d / first.Duration,
                    (first.CpuUsage.GetValueOrDefault().TotalMicroSeconds / 1000d).ToString("n3"),
                    (1000d * first.CpuUsage.GetValueOrDefault().UserUsage.TotalSeconds).ToString("n3"),
                    (1000d * first.CpuUsage.GetValueOrDefault().KernelUsage.TotalSeconds).ToString("n3")
                );
            }

            return ret;
        }

        public static void ReportToFile(string fullName)
        {
            Thread t = new Thread(() =>
            {
                long prevTimestamp = long.MinValue;
                while (true)
                {
                    var nextTimestamp = AdvancedMiniProfilerReport.Instance.Timestamp;
                    var toSleep = 222;
                    if (nextTimestamp != prevTimestamp)
                    {
                        prevTimestamp = nextTimestamp;
                        using (AdvancedMiniProfiler.Step("Advanced Profiler", "Update this report"))
                        {
                            var consoleTable = AdvancedMiniProfilerReport.Instance.AsConsoleTable();
                            try
                            {
                                using (FileStream fs = new FileStream(fullName, FileMode.Create, FileAccess.Write,
                                    FileShare.ReadWrite))
                                using (StreamWriter wr = new StreamWriter(fs, new UTF8Encoding(false)))
                                {
                                    wr.WriteLine(consoleTable.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                var logMessage = $"Unable to store mini profiler report as '{fullName}' file. {ex.GetExceptionDigest()}";
                                FirstRound.RunOnly(() => Console.WriteLine(logMessage),
                                    3, "Unable to store mini profiler report"
                                );
                            }
                        }

                        toSleep = 4000;
                    }
                    Thread.Sleep(toSleep);
                }
            }) { IsBackground = true};
            t.Start();
        }
    }
    
    public class AdvancedMiniProfiler
    {
        /*
        private static Lazy<object> PreJit = new Lazy<object>(() =>
        {
            var path = new AdvancedMiniProfilerKeyPath("AdvancedMiniProfiler", "PreJit");
            using (var step = new AdvancedMiniProfilerStep(path))
            {
            }

            return new object();
        }, LazyThreadSafetyMode.ExecutionAndPublication);
        */

        public static AdvancedMiniProfilerStep Step(params string[] keyPath)
        {
            return Step(new AdvancedMiniProfilerKeyPath(keyPath));
        }
        public static AdvancedMiniProfilerStep Step(AdvancedMiniProfilerKeyPath keyPath)
        {
            // object preJitted = PreJit.Value;
            return new AdvancedMiniProfilerStep(keyPath);
        }
    }

    public class AdvancedMiniProfilerStep : IDisposable
    {
        public AdvancedMiniProfilerKeyPath KeyPath { get; }
        private Stopwatch Stopwatch;
        private CpuUsage? CpuUsageOnStart;

        public AdvancedMiniProfilerStep(AdvancedMiniProfilerKeyPath keyPath)
        {
            KeyPath = keyPath;
            CpuUsageOnStart = CpuUsage.GetByThread();
            Stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            var duration = Stopwatch.ElapsedTicks / (double) Stopwatch.Frequency;
            CpuUsage? cpuUsage = null;
            if (CpuUsageOnStart.HasValue)
            {
                cpuUsage = CpuUsage.GetByThread();
                if (cpuUsage.HasValue)
                    cpuUsage = CpuUsage.Substruct(cpuUsage.Value, CpuUsageOnStart.Value);
            }
            
            AdvancedMiniProfilerReport.Instance.Add(KeyPath, new AdvancedMiniProfilerMetrics()
            {
                Count = 1,
                Duration = duration,
                CpuUsage = cpuUsage
            });
        }
    }
}