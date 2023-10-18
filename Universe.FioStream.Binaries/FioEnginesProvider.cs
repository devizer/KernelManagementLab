using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Universe.FioStream.Binaries
{
    public class FioEnginesProvider
    {
        // public static bool EnableFioDownload = true;

        // Depends on available memory - thus it is static
        public static int DiscoveryThreadsLimit { get; set; } = 12;
        private readonly FioFeaturesCache FeaturesCache;
        private readonly IPicoLogger Logger;

        private Dictionary<string, EngineInternals> TheState = new Dictionary<string, EngineInternals>();
        private readonly object SyncState = new object();
        private string[] TargetEngines => TargetEnginesByPlatform.Value;

        private const string LinuxEngines = "io_uring,libaio,posixaio,pvsync2,pvsync,vsync,psync,sync,mmap";
        private const string WindowsEngines = "windowsaio,psync,sync,mmap";
        private const string OsxEngines = "posixaio,pvsync2,pvsync,vsync,psync,sync,mmap";

        private static Lazy<string[]> TargetEnginesByPlatform = new Lazy<string[]>(() =>
        {
            string rawTargetEngines;
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                rawTargetEngines = WindowsEngines;
            else if (CrossInfo.ThePlatform == CrossInfo.Platform.MacOSX)
                rawTargetEngines = OsxEngines;
            else
                rawTargetEngines = LinuxEngines;

            return rawTargetEngines.Split(',');
        });
        
        public FioEnginesProvider(FioFeaturesCache featuresCache, IPicoLogger logger)
        {
            FeaturesCache = featuresCache;
            Logger = logger;
        }

        public List<Engine> GetEngines()
        {
            KeyValuePair<string, EngineInternals>[] stateCopy;
            lock (SyncState)
                stateCopy = TheState.ToArray();

            int IndexOfEngine(string engine)
            {
                if (TargetEngines == null) return int.MaxValue;
                int ret = Array.IndexOf(TargetEngines, engine);
                return ret >= 0 ? ret : int.MaxValue;
            }

            return stateCopy.Select(pair => new Engine()
            {
                IdEngine = pair.Key,
                Executable = pair.Value.Executable,
                Version = pair.Value.Version,
                LogDetails = pair.Value.LogDetails,
            })
                .OrderBy(x => IndexOfEngine(x.IdEngine))
                .ToList();
        }

        public class Engine
        {
            public string IdEngine { get; set; }
            public string Executable { get; set; }
            public Version Version { get; set; }
            
            public string LogDetails { get; set; }

            public override string ToString()
            {
                return $"{IdEngine}-v{Version}: {Executable}{(string.IsNullOrEmpty(LogDetails) ? "" : ", " + LogDetails)}";
            }
        }

        internal class EngineInternals
        {
            public string Executable;
            public Version Version;
            public FioFeatures Features;
            public string LogDetails { get; set; }
        }

        public void Discovery()
        {
            Logger?.LogInfo($"Discovering supported FIO Engines on {CrossInfo.ThePlatform}: [{string.Join(",", TargetEngines)}]");

            ConcurrentDictionary<string, Candidates.Info> candidatesByEngines = new ConcurrentDictionary<string, Candidates.Info>();

            Stopwatch sw = Stopwatch.StartNew();
            List<Candidates.Info> candidates = Candidates.GetCandidates();
            candidates.Insert(0, Candidates.Info.LocalFio);

            bool IsAllIsFound() => TargetEngines.Length == candidatesByEngines.Count; 

            void TryCandidate(Candidates.Info bin)
            {
                if (IsAllIsFound()) return;

                FioFeatures features = null;
                try
                {
                    features = FeaturesCache[bin];
                }
                catch (Exception ex)
                {
                    this.Logger.LogWarning($"Skipped. Unable to obtain fio ${bin.Name} from {bin.Url}{Environment.NewLine}{ex}");
                    return;
                }
                
                var engines = features.EngineList;
                if (engines == null) return;
                var version = features.Version;
                if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                    if (!engines.Contains("windowsaio"))
                        engines = engines.Concat(new[] {"windowsaio"}).ToArray();

                var toFind = TargetEngines
                    .Where(x => !candidatesByEngines.ContainsKey(x))
                    .Where(x => engines.Contains(x));

                foreach (var engine in toFind)
                {
                    if (IsAllIsFound()) return;
                    Logger?.LogInfo($"Checking engine [{engine}] for [{bin.Name}]");
                    bool isEngineSupported = features.IsEngineSupported(engine);
                    if (isEngineSupported)
                    {
                        candidatesByEngines[engine] = bin;
                        lock (SyncState)
                        {
                            // check if already found
                            TheState.TryGetValue(engine, out var found);
                            if (found == null || (version != null && found.Version != null && version.CompareTo(found.Version) > 0))
                            {
                                TheState[engine] = new EngineInternals()
                                {
                                    Executable = features.Executable,
                                    Version = version,
                                    Features = features,
                                    LogDetails = $"at {sw.Elapsed.TotalSeconds:0.0} sec"
                                };
                                
                                {
                                    var todo = $"{(TargetEngines.Length - candidatesByEngines.Count)}";
                                    var progress = $"{candidatesByEngines.Count}/{TargetEngines.Length} {sw.Elapsed} {engine}: {bin.Name}";
                                    Logger?.LogInfo(progress);
                                }
                            }
                        }
                    }
                }
            }
            
            // Run In Parallel
            var threadsByCpuCount = new[] {4, 8, 12};
            var threads = threadsByCpuCount[Math.Min(threadsByCpuCount.Length, Environment.ProcessorCount) - 1];
            threads = Math.Min(threads, DiscoveryThreadsLimit);
            threads = Math.Max(threads, 1);
            // threads = 1;
            ParallelOptions parallelOptions = new ParallelOptions() {MaxDegreeOfParallelism = threads,};
            Logger?.LogInfo($"Checking [{candidates.Count}] candidates for [{Candidates.PosixSystem}] running on [{Candidates.PosixMachine}] cpu using up to {threads} threads");
            Parallel.ForEach(candidates, parallelOptions, TryCandidate);

            // Show Recap
            var nl = Environment.NewLine;
            List<Engine> enginesResult = this.GetEngines();
            var joined = string.Join(nl, enginesResult.Select(x => $" - {x}").ToArray());
            Logger?.LogInfo($"Found {enginesResult.Count} supported fio engines in {sw.Elapsed.TotalSeconds:0.0} seconds: {nl}{joined}");
        }

    }
}
