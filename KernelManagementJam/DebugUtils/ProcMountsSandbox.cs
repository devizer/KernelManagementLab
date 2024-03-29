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
            
            // Only for logs below
            Func<DriveDetails, string> driveDetailsForHuman = driveDetails =>
            {
                var mountPath = driveDetails.MountEntry?.MountPath;
                var fileSystem = driveDetails.MountEntry?.FileSystem;
                if (string.IsNullOrEmpty(mountPath) || string.IsNullOrEmpty(fileSystem)) return null;
                string humanSize = driveDetails.TotalSize > 0 ? Formatter.FormatBytes(driveDetails.TotalSize) : null;
                return $"{mountPath} ({fileSystem}{(humanSize == null ? "" : $", {humanSize}")})";
            };
            
            var humanInfo = analyz.Details
                .Where(x => driveDetailsForHuman(x) != null)
                .OrderBy(x => x.MountEntry?.MountPath)
                .Select(x => driveDetailsForHuman(x));

            Console.WriteLine($"Preliminary system volumes: {Environment.NewLine}{string.Join(Environment.NewLine, humanInfo.Select(x => $"  • {x}"))}");
            // Console.WriteLine(logDetails);
            // Console.WriteLine(analyz.RawDetailsLog);

            DebugDumper.DumpText(logDetails + Environment.NewLine + analyz.RawDetailsLog, "ProcMountsAnalyzer.report");
            DebugDumper.Dump(analyz, "ProcMountsAnalyzer.json");

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
            }

        }
    }
}
