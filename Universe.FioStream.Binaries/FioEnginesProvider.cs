using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Universe.FioStream.Binaries
{
    public class FioEnginesProvider
    {
        private readonly FioFeaturesCache FeaturesCache;
        private readonly IPicoLogger Logger;

        private Dictionary<string, EngineInternals> TheState = new Dictionary<string, EngineInternals>();
        private readonly object SyncState = new object();
        private string[] TargetEngines;

        private const string LinuxEngines = "io_uring,libaio,posixaio,pvsync2,pvsync,vsync,psync,sync,mmap";
        private const string WindowsEngines = "windowsaio,psync,sync,mmap";
        private const string OsxEngines = "posixaio,pvsync2,pvsync,vsync,psync,sync,mmap";
        
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
            })
                .OrderBy(x => IndexOfEngine(x.IdEngine))
                .ToList();
        }

        public class Engine
        {
            public string IdEngine { get; set; }
            public string Executable { get; set; }
            public Version Version { get; set; }

            public override string ToString()
            {
                return $"{IdEngine}-v{Version}: {Executable}";
            }
        }

        internal class EngineInternals
        {
            public string Executable;
            public Version Version;
            public FioFeatures Features;
        }

        public void Discovery()
        {
            string rawTargetEngines;
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                rawTargetEngines = WindowsEngines;
            else if (CrossInfo.ThePlatform == CrossInfo.Platform.MacOSX)
                rawTargetEngines = OsxEngines;
            else
                rawTargetEngines = LinuxEngines;

            TargetEngines = rawTargetEngines.Split(','); 
            
            Logger?.LogInfo($"Discovery Supported FIO Engines on {CrossInfo.ThePlatform}: {rawTargetEngines}");

            Dictionary<string, Candidates.Info> candidatesByEngines = new Dictionary<string, Candidates.Info>();

            Stopwatch sw = Stopwatch.StartNew();
            List<Candidates.Info> candidates = Candidates.GetCandidates();
            candidates.Insert(0, new Candidates.Info() { Name = "fio", Url = "skip://downloading"});

            void TryCandidate(Candidates.Info bin)
            {
                if (TargetEngines.Length == candidatesByEngines.Count) return;

                FioFeatures features = FeaturesCache[bin];
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
                    Logger?.LogInfo($"Checking engine [{engine}] for [{bin.Name}]");
                    bool isEngineSupported = features.IsEngineSupported(engine);
                    if (isEngineSupported)
                    {
                        candidatesByEngines[engine] = bin;
                        lock (SyncState)
                        {
                            TheState[engine] = new EngineInternals()
                            {
                                Executable = features.Executable,
                                Version = version,
                                Features = features
                            };
                        }

                        {
                            var todo = $"{(TargetEngines.Length - candidatesByEngines.Count)}";
                            Logger?.LogInfo(
                                $"{candidatesByEngines.Count}/{TargetEngines.Length} {sw.Elapsed} {engine}: {bin.Name}");
                        }
                    }
                }
            }
            
            var threadsByCpuCount = new[] {4, 8, 12};
            var threads = threadsByCpuCount[Math.Min(threadsByCpuCount.Length, Environment.ProcessorCount) - 1];
            ParallelOptions parallelOptions = new ParallelOptions() {MaxDegreeOfParallelism = threads,};
            Logger?.LogInfo($"Checking [{candidates.Count}] candidates for [{Candidates.PosixSystem}] running on [{Candidates.PosixMachine}] cpu using up to {threads} threads");
            Parallel.ForEach(candidates, parallelOptions, TryCandidate);

            var nl = Environment.NewLine;
            var enginesResult = this.GetEngines();
            var joined = string.Join(nl, enginesResult.Select(x => $" - {x}").ToArray());
            Logger?.LogInfo($"Found {enginesResult.Count} supported engines: {nl}{joined}");
        }

    }
}