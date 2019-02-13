using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Unix;
using Newtonsoft.Json;

namespace KernelManagementJam
{
    // Similar to System.IO.DriveInfo and Mono.Unix.UnixDriveInfo

    public class ProcMountsAnalyzer
    {
        public List<DriveDetails> Details { get; private set; }
        public List<MountEntry> ArgMountEntries { get; private set; }

        [JsonIgnore]
        public string RawDetailsLog { get; private set; }

        public static ProcMountsAnalyzer Create(IEnumerable<MountEntry> mountEntries, bool skipDetailsLog = false)
        {
            Stopwatch startAt = Stopwatch.StartNew();
            var report = new ConsoleTable(
                "Device", "Block", "FS", "Path", // from /proc/mounts
                "", "-Free", "-Total", "Type", "msec", ""
            );

            var allDetails = new List<DriveDetails>();
            foreach (var mount in mountEntries)
            {
                Exception error = null;
                string driveInfo = null;
                var sw = Stopwatch.StartNew();
                DriveDetails details = null;
                if (FileSystemHelper.Exists(mount.MountPath))
                {
                    try
                    {
                        // TRY System.IO.DriveInfo
                        var di = new DriveInfo(mount.MountPath);
                        details = new DriveDetails
                        {
                            MountEntry = mount,
                            IsReady = di.IsReady,
                            Format = di.DriveType.ToString(),
                            FreeSpace = di.TotalFreeSpace,
                            TotalSize = di.TotalSize
                        };
                    }
                    catch (Exception exNet)
                    {
                        error = exNet;
                        // TRY UnixDriveInfo 
                        try
                        {
                            var di = new UnixDriveInfo(mount.MountPath);
                            details = new DriveDetails
                            {
                                MountEntry = mount,
                                IsReady = di.IsReady && di.TotalSize > 0,
                                Format = di.DriveType.ToString(),
                                FreeSpace = di.TotalFreeSpace,
                                TotalSize = di.TotalSize
                            };
                            error = null;
                        }
                        catch (Exception exUnix)
                        {
                            // error = exUnix;
                        }
                    }
                }
                else
                {
                    error = new Exception("MountPath doesn't exist");
//                    details = new DriveDetails
//                    {
//                        MountEntry = mount,
//                        IsReady = false,
//                    };

                }

                var msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                if (details != null)
                {
                    // Console.WriteLine($"Try {mount.Device}");
                    details.BlockDeviceResolved = null;
                    var resolved = FileSystemHelper.Resolve(mount.Device);
                    if (FileSystemHelper.IsBlockDevice(resolved))
                        details.BlockDeviceResolved = resolved;


                    if (!skipDetailsLog)
                        report.AddRow(
                            mount.Device, details.BlockDeviceResolved, mount.FileSystem, mount.MountPath,
                            details.IsReady ? "OK" : "--", Formatter.FormatBytes(details.FreeSpace), Formatter.FormatBytes(details.TotalSize),
                            details.Format, $"{msec:f2}"
                        );

                    allDetails.Add(details);
                }
                else if (error != null)
                {
                    var errorInfo = error.GetType().Name + ": " + error.Message.Replace(Environment.NewLine, " ");

                    if (!skipDetailsLog)
                        report.AddRow(
                            mount.Device, null, mount.FileSystem, mount.MountPath,
                            "--", "", "",
                            "", $"{msec:f2}", errorInfo
                        );
                }
            }

            var totalMsec = startAt.ElapsedTicks * 1000d / Stopwatch.Frequency;
            var footer = $"Total time taken: {totalMsec:n1} milliseconds";
            var log = skipDetailsLog ? footer : report + Environment.NewLine + Environment.NewLine + footer;
            var ret = new ProcMountsAnalyzer
            {
                ArgMountEntries = mountEntries.ToList(),
                Details = allDetails,
                RawDetailsLog = log
            };

            return ret;
        }
    }
}