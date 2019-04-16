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
                    Iteration();
                    IsFirstIterationReady.Set();
                    PreciseTimer.Shutdown.WaitOne(1000);
                }
            });
        }

        static void Iteration()
        {
            bool isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            IList<MountEntry> mounts = ProcMountsParser.Parse(isWin ? "mounts" : "/proc/mounts").Entries;

            ProcMountsAnalyzer analyz = ProcMountsAnalyzer.Create(mounts, skipDetailsLog: true);

            Mounts = analyz.Details;
            
            DebugDumper.Dump(analyz, "ProcMountsAnalyzer.json");
            DebugDumper.Dump(analyz, "ProcMountsAnalyzer.min.json", true);

            // Group by
            Func<DriveDetails, bool> isNetwork = x => x.IsNetworkShare;
            Func<DriveDetails, bool> isRam = x => x.IsTmpFs;
            Func<DriveDetails, bool> isBlock = x => x.MountEntry.Device.StartsWith("/dev/");
            var args = new[]
            {
                new {Title = "Vols-Block", Predicate = isBlock},
                new {Title = "Vols-Net", Predicate = isNetwork},
                new {Title = "Vols-Ram", Predicate = isRam},
            };

            foreach (var volType in args)
            {
                var filtered = analyz.Details.Where(volType.Predicate).ToList();
                DebugDumper.Dump(filtered, volType.Title + ".json");
                DebugDumper.Dump(filtered, volType.Title + ".min.json", minify: true);
            }
        }
        
    }
}