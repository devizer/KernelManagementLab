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
        private readonly Lazy<int> _HashCode;
        private readonly Lazy<string> _ToString;
        public string[] Path { get; }

        public AdvancedMiniProfilerKeyPath()
        {
            _ToString = new Lazy<string>(() =>
            {
                // const string arrow = " \x27a1 ";
                const string arrow = " \x2192 ";
                return string.Join(arrow, Path ?? new string[0]);
            }, LazyThreadSafetyMode.ExecutionAndPublication);
            
            _HashCode = new Lazy<int>(() =>
            {
                if (Path == null) return 0;
                int ret = 0;
                unchecked
                {
                    foreach (var p in Path)
                        ret = ret * 397 ^ (p?.GetHashCode() ?? 0);
                }

                return ret;

            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public AdvancedMiniProfilerKeyPath(params string[] path) : this()
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            for(int i=0, l=path.Length; i<l; i++)
                if (path[i] == null) throw new ArgumentException($"path's element #{i} is null", nameof(path));
        }

        public AdvancedMiniProfilerKeyPath Child(string childName)
        {
            if (childName == null) throw new ArgumentNullException(nameof(childName));
            return new AdvancedMiniProfilerKeyPath(Path.Concat(new[] {childName}).ToArray());
        }

        public override string ToString() => _ToString.Value;

        protected bool Equals(AdvancedMiniProfilerKeyPath other)
        {
            if (_HashCode.Value != other._HashCode.Value) return false;
            
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
            return _HashCode.Value;
        }

        class TempTree : Dictionary<AdvancedMiniProfilerKeyPath, TempTree>
        {
            // null for sub tree
            public AdvancedMiniProfilerKeyPath Leaf;
        }
        
        public static List<Node<AdvancedMiniProfilerKeyPath>> AsTree(IEnumerable<AdvancedMiniProfilerKeyPath> plainList)
        {
            TempTree root = new TempTree();
            foreach (var plainItem in plainList)
            {
                var parent = root;
                for (int i = 0, l=plainItem.Path.Length; i < l; i++)
                {
                    AdvancedMiniProfilerKeyPath partialPath = new AdvancedMiniProfilerKeyPath(plainItem.Path.Take(i+1).ToArray());
                    TempTree current = parent.GetOrAdd(partialPath, key => new TempTree());
                    parent = current;
                }
            }
            
            if (plainList.Count() >= 33)
            {
                var breakHere = "ok";
            }

            
            List<Node<AdvancedMiniProfilerKeyPath>> ret = new List<Node<AdvancedMiniProfilerKeyPath>>();
            EnumSubTree(root, ret);
            return ret;
        }

        private static void EnumSubTree(TempTree treeNode, List<Node<AdvancedMiniProfilerKeyPath>> nodes)
        {
            
            foreach (var pair in treeNode)
            {
                AdvancedMiniProfilerKeyPath keyPath = pair.Key;
                TempTree subTree = pair.Value;
                Node<AdvancedMiniProfilerKeyPath> subNode = new Node<AdvancedMiniProfilerKeyPath>()
                {
                    State = keyPath,
                    Name = keyPath.Path.Last(),
                };
                nodes.Add(subNode);
                EnumSubTree(subTree, subNode.Children);
            }

            var sorted = nodes.OrderBy(x => x.Name).ToList();
            nodes.Clear();
            nodes.AddRange(sorted);
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
        private const int REPORT_UPDATE_DELAY = 30000;
        private Dictionary<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics> Report = new Dictionary<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics>();
        private Dictionary<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics> FirstCall = new Dictionary<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics>();
        readonly object Sync = new object();
        static readonly  AdvancedMiniProfilerReport _Instance = new AdvancedMiniProfilerReport();
        public static AdvancedMiniProfilerReport Instance => _Instance;
        long _Timestamp = 0;

        static AdvancedMiniProfilerReport()
        {
        }

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
                AdvancedMiniProfilerMetrics prev = Report.GetOrAdd(path, key => new AdvancedMiniProfilerMetrics());
                prev.Duration += metrics.Duration;
                prev.CpuUsage += metrics.CpuUsage; 
                prev.Count += 1;
            }
            else
                FirstCall[path] = metrics;

            _Timestamp++;
        }

        public ConsoleTable AsTreeTable()
        {
            KeyValuePair<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics>[] reportCopyRaw, firstCallCopyRaw; 
            lock (Sync)
            {
                reportCopyRaw = this.Report.ToArray();
                firstCallCopyRaw = this.FirstCall.ToArray();
            }

            reportCopyRaw = reportCopyRaw.OrderBy(x => x.Key.ToString()).ToArray();
            var reportCopy = reportCopyRaw.ToDictionary(x => x.Key, x => x.Value);
            var firstCallCopy = firstCallCopyRaw.ToDictionary(x => x.Key, x => x.Value);

            List<Node<AdvancedMiniProfilerKeyPath>> rootKeys = AdvancedMiniProfilerKeyPath.AsTree(reportCopyRaw.Select(x => x.Key));
            List<KeyValuePair<AdvancedMiniProfilerKeyPath, string>> orderedKeys = new List<KeyValuePair<AdvancedMiniProfilerKeyPath, string>>();

            void Enum1(List<Node<AdvancedMiniProfilerKeyPath>> nodes)
            {
                foreach (var node in nodes)
                {
                    orderedKeys.Add(new KeyValuePair<AdvancedMiniProfilerKeyPath, string>(node.State, node.AscII));
                    Enum1(node.Children);
                }
            }
            AscIITreeDiagram<AdvancedMiniProfilerKeyPath>.PopulateAscII(rootKeys);
            Enum1(rootKeys);
            
            if (reportCopyRaw.Length >= 33)
            {
                var breakHere = "ok";
            }

            
            ConsoleTable ret = new ConsoleTable("Path", "-N", 
                "-Duration", "-CPU (%)", "-CPU (\x3bcs)", "-User", "-Kernel",
                "-1st Duration", "-1st CPU (%)", "-1st CPU (\x3bcs)", "-1st User", "-1st Kernel");

            var zeroMetrics = new AdvancedMiniProfilerMetrics();
            foreach (var pair in orderedKeys)
            {
                var path = pair.Key;
                var pathAsString = pair.Value;
                // var total = reportCopyRaw.FirstOrDefault(x => x.Key.Equals(path)).Value ?? zeroMetrics;
                reportCopy.TryGetValue(path, out var total);
                
                // var first = firstCallCopy.FirstOrDefault(x => x.Key.Equals(path)).Value ?? zeroMetrics;
                if (!firstCallCopy.TryGetValue(path, out var first))
                    first = zeroMetrics;
                
                if (total == null) ret.AddRow(pathAsString);
                else
                ret.AddRow(pathAsString, total.Count,
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

        public ConsoleTable AsConsoleTable()
        {
            KeyValuePair<AdvancedMiniProfilerKeyPath, AdvancedMiniProfilerMetrics>[] reportCopy, firstCallCopyRaw; 
            lock (Sync)
            {
                reportCopy = this.Report.ToArray();
                firstCallCopyRaw = this.FirstCall.ToArray();
            }

            reportCopy = reportCopy.OrderBy(x => x.Key.ToString()).ToArray();
            var firstCallCopy = firstCallCopyRaw.ToDictionary(x => x.Key, x => x.Value);
            
            ConsoleTable ret = new ConsoleTable("Path", "-N", 
                "-Duration", "-CPU (%)", "-CPU (\x3bcs)", "-User", "-Kernel",
                "-1st Duration", "-1st CPU (%)", "-1st CPU (\x3bcs)", "-1st User", "-1st Kernel");

            var zeroMetrics = new AdvancedMiniProfilerMetrics();
            foreach (var pair in reportCopy)
            {
                var path = pair.Key;
                var total = pair.Value;
                // var first = firstCallCopy.FirstOrDefault(x => x.Key.Equals(path)).Value ?? zeroMetrics;
                if (!firstCallCopy.TryGetValue(path, out var first))
                    first = zeroMetrics;
                
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

        public static void InstantSaveReport(string fullName)
        {
            try
            {
                var directoryName = Path.GetDirectoryName(fullName);
                if (Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
            }
            catch
            {
            }

            SaveReportImpl(fullName);
        }

        private static void SaveReportImpl(string fullName)
        {
            var consoleTable = AdvancedMiniProfilerReport.Instance.AsConsoleTable();
            var treeTable = AdvancedMiniProfilerReport.Instance.AsTreeTable();
            try
            {
                using (FileStream fs = new FileStream(fullName, FileMode.Create, FileAccess.Write,
                    FileShare.ReadWrite))
                using (StreamWriter wr = new StreamWriter(fs, new UTF8Encoding(false)))
                {
                    wr.WriteLine(treeTable.ToString());
                    wr.WriteLine();
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

        public static void ReportToFile(string fullName)
        {
            Thread t = new Thread(() =>
            {
                Thread.Sleep(5000);
                long prevTimestamp = long.MinValue;
                while (true)
                {
                    var nextTimestamp = AdvancedMiniProfilerReport.Instance.Timestamp;
                    var toSleep = 222;
                    if (nextTimestamp != prevTimestamp)
                    {
                        prevTimestamp = nextTimestamp;
                        using (AdvancedMiniProfiler.Step("Advanced Profiler (update this report)"))
                        {
                            SaveReportImpl(fullName);
                        }

                        toSleep = REPORT_UPDATE_DELAY;
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