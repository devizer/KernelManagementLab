using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;

namespace Universe.Dashboard.Agent
{
    // Consistency of optional.
    // Iteration may be slow for offline disks
    public class MountsDataSource
    {
        private static readonly AdvancedMiniProfilerKeyPath 
            BaseProfilerKey = new AdvancedMiniProfilerKeyPath("Kernel Stat", "MountsDataSource.Iteration()");
        
        static List<DriveDetails> _Mounts;
        static readonly object Sync = new object();
        public static ManualResetEvent IsFirstIterationReady = new ManualResetEvent(false);

        static MountsDataSource()
        {
            LaunchListener();
        }
        
        public static List<DriveDetails> Mounts
        {
            get
            {
                IsFirstIterationReady.WaitOne();
                lock (Sync) return _Mounts;
            }
            private set
            {
                lock (Sync) _Mounts = value;
            }
        }

        private static void LaunchListener()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                while (!PreciseTimer.Shutdown.WaitOne(0))
                {
                    using(AdvancedMiniProfiler.Step(BaseProfilerKey))
                        Iteration();
                    
                    IsFirstIterationReady.Set();
                    PreciseTimer.Shutdown.WaitOne(1000);
                }
            });
        }

        static void Iteration()
        {
            
            IList<MountEntry> mounts;
            using(AdvancedMiniProfiler.Step(BaseProfilerKey.Child("1. Parse /proc/mounts")))
                mounts = ProcMountsParser.Parse(FakeRootFs.Transform("/proc/mounts")).Entries;

            ProcMountsAnalyzer analyz;
            using (AdvancedMiniProfiler.Step(BaseProfilerKey.Child("2. ProcMountsAnalyzer.Details")))
            {
                analyz = ProcMountsAnalyzer.Create(mounts, skipDetailsLog: true);
                Mounts = analyz.Details;
            }

            DebugDumper.Dump(analyz, "ProcMountsAnalyzer.json");
            DebugDumper.Dump(analyz, "ProcMountsAnalyzer.min.json", true);

            // Group by
            Func<DriveDetails, bool> isNetwork = x => x.IsNetworkShare;
            Func<DriveDetails, bool> isRam = x => x.IsTmpFs;
            Func<DriveDetails, bool> isBlock = x => x.MountEntry.Device.StartsWith("/dev/");
            var args = new[]
            {
                new {Title = "Block-Volumes", Predicate = isBlock},
                new {Title = "Net-Volumes", Predicate = isNetwork},
                new {Title = "Ram-Volumes", Predicate = isRam},
            };

            int stepCounter = 2;
            foreach (var volType in args)
            {
                using (AdvancedMiniProfiler.Step(BaseProfilerKey.Child($"{++stepCounter}. Filter by {volType.Title}")))
                {
                    var filtered = analyz.Details.Where(volType.Predicate).ToList();
                    DebugDumper.Dump(filtered, volType.Title + ".json");
                    DebugDumper.Dump(filtered, volType.Title + ".min.json", minify: true);
                }
            }
        }
        
    }
}