﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Mono.Unix;
using Newtonsoft.Json;

namespace KernelManagementJam
{
    // Similar to System.IO.DriveInfo and Mono.Unix.UnixDriveInfo
    public class DriveDetails
    {
        private const StringComparison CMP = System.StringComparison.CurrentCultureIgnoreCase;
        public MountEntry MountEntry { get; set; }
        public bool IsReady { get; set; }
        public long FreeSpace { get; set; }
        public long TotalSize { get; set; }
        public string Format { get; set; }

        public bool IsTmpFs => (MountEntry?.FileSystem ?? "").EndsWith("tmpfs", StringComparison.InvariantCultureIgnoreCase);

        public bool IsNetworkShare
        {
            get
            {
                return IsNfs || IsSsh || IsCifs || IsFtp || IsWebDav;
            }
        }

        public bool IsFtp
        {
            get
            {
                return (MountEntry?.Device ?? "").IndexOf("ftpfs#", CMP) >= 0 || (MountEntry?.Device ?? "").IndexOf("ftp://", CMP) >= 0 ||
                       (MountEntry?.Device ?? "").IndexOf("ftps://", CMP) >= 0;
            }
        }

        public bool IsCifs
        {
            get { return (MountEntry?.FileSystem ?? "").IndexOf("cifs", CMP) >= 0; }
        }

        public bool IsSsh
        {
            get { return (MountEntry?.FileSystem ?? "").IndexOf("sshfs", CMP) >= 0; }
        }

        public bool IsNfs
        {
            get { return (MountEntry?.FileSystem ?? "").StartsWith("nfs", CMP); }
        }

        public bool IsWebDav
        {
            get
            {
                try
                {
                    Uri u = new Uri(MountEntry?.Device ?? "");
                    return !string.IsNullOrEmpty(u.Host) && ("http".Equals(u.Scheme, CMP) || "https".Equals(u.Scheme, CMP));
                }
                catch
                {
                    return false;
                }
            }
        }
    }

    public class ProcMountsAnalyzer
    {
        public List<DriveDetails> Details { get; private set; }
        public List<MountEntry> ArgMountEntries { get; private set; }

        [JsonIgnore]
        public string RawDetailsLog { get; private set; }

        public static ProcMountsAnalyzer Create(IEnumerable<MountEntry> mountEntries)
        {
            Stopwatch startAt = Stopwatch.StartNew();
            var report = new ConsoleTable(
                "Device", "FS", "Path", // from /proc/mounts
                "", "-Free", "-Total", "Type", "msec", ""
            );

            var allDetails = new List<DriveDetails>();
            foreach (var mount in mountEntries)
            {
                Exception error = null;
                string driveInfo = null;
                var sw = Stopwatch.StartNew();
                DriveDetails details = null;
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

                var msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                if (details != null)
                {
                    report.AddRow(
                        mount.Device, mount.FileSystem, mount.MountPath,
                        details.IsReady ? "OK" : "--", Formatter.FormatBytes(details.FreeSpace), Formatter.FormatBytes(details.TotalSize),
                        details.Format, $"{msec:f2}"
                    );

                    allDetails.Add(details);
                }
                else if (error != null)
                {
                    var errorInfo = error.GetType().Name + ": " + error.Message.Replace(Environment.NewLine, " ");
                    report.AddRow(
                        mount.Device, mount.FileSystem, mount.MountPath,
                        "--", "", "",
                        "", $"{msec:f2}", errorInfo
                    );
                }
            }

            var totalMsec = startAt.ElapsedTicks * 1000d / Stopwatch.Frequency;
            var footer = $"Total time taken: {totalMsec:n1} milliseconds";
            var ret = new ProcMountsAnalyzer
            {
                ArgMountEntries = mountEntries.ToList(),
                Details = allDetails,
                RawDetailsLog = report + Environment.NewLine + Environment.NewLine + footer
            };

            return ret;
        }
    }
}