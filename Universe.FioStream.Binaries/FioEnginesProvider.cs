using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Universe.FioStream.Binaries
{
    public class FioEnginesProvider
    {
        private readonly FioFeaturesCache FeaturesCache;
        private readonly IPicoLogger Logger;

        private Dictionary<string, EngineInternals> TheState = new Dictionary<string, EngineInternals>();
        private readonly object SyncState = new object();
        
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

            return stateCopy.Select(pair => new Engine()
            {
                IdEngine = pair.Key,
                Executable = pair.Value.Executable,
                Version = pair.Value.Version,
            }).ToList();
        }

        public class Engine
        {
            public string IdEngine { get; set; }
            public string Executable { get; set; }
            public Version Version { get; set; }
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

            var targetEngines = rawTargetEngines.Split(','); 
            
            Logger?.LogInfo($"Discovery Supported FIO Engines on {CrossInfo.ThePlatform}: {rawTargetEngines}");

            Dictionary<string, Candidates.Info> candidatesByEngines = new Dictionary<string, Candidates.Info>();

            Stopwatch sw = Stopwatch.StartNew();
            List<Candidates.Info> candidates = Candidates.GetCandidates();
            candidates.Insert(0, new Candidates.Info() { Name = "fio", Url = "skip://downloading"});
            Logger?.LogInfo($"Checking [{candidates.Count}] candidates for [{Candidates.PosixSystem}] running on [{Candidates.PosixMachine}] cpu");
            foreach (var bin in candidates)
            {
                if (targetEngines.Length == candidatesByEngines.Count) break;
                
                FioFeatures features = FeaturesCache[bin];
                var engines = features.EngineList;
                if (engines == null) continue;
                var version = features.Version;
                if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                    if (!engines.Contains("windowsaio"))
                        engines = engines.Concat(new[] {"windowsaio"}).ToArray();

                var toFind = targetEngines
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
                            var todo = $"{(targetEngines.Length - candidatesByEngines.Count)}";
                            Logger?.LogInfo($"{candidatesByEngines.Count}/{targetEngines.Length} {sw.Elapsed} {engine}: {bin.Name}");
                        }
                    }
                }
            }

            var nl = Environment.NewLine;
            var joined = string.Join(nl, candidatesByEngines.Select(x => $"{x.Key}: {x.Value.Name}").ToArray());
            Logger?.LogInfo($"{nl}{nl}Found {candidatesByEngines.Count} supported engines: for engines{nl}{joined}");
            var exit = "ok";
        }

    }
}