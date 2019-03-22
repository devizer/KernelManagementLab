using System;
using System.Collections.Generic;
using System.Linq;
using KernelManagementJam.DebugUtils;

namespace KernelManagementJam
{
    public class ProcMountsSandbox
    {
        public static void DumpProcMounts()
        {
            bool isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            IList<MountEntry> mounts = ProcMountsParser.Parse(isWin ? "mounts" : "/proc/mounts").Entries;

            ProcMountsAnalyzer analyz = ProcMountsAnalyzer.Create(mounts);
            string logDetails = analyz.RawDetailsLog;
            analyz = ProcMountsAnalyzer.Create(mounts, skipDetailsLog: true);
            Console.WriteLine(logDetails);
            Console.WriteLine(analyz.RawDetailsLog);

            DebugDumper.DumpText(logDetails + Environment.NewLine + analyz.RawDetailsLog, "debug-dumps/ProcMountsAnalyzer.report");
            DebugDumper.Dump(analyz, "debug-dumps/ProcMountsAnalyzer.js");

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
                DebugDumper.Dump(filtered, "debug-dumps/" + volType.Title + ".js");
            }

        }

    }
}