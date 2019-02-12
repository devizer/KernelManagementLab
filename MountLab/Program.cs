using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KernelManagementJam;
using Mono.Unix;

namespace MountLab
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                var blockDevices = SysBlocksReader.GetSnapshot();
                DebugDumper.Dump(blockDevices, "SysBlockSnapshot.js");
            }

            DumpProcMounts();
            DumpManagedDrives();
            DumpUnixDrives();
            DumpUnixDriveInfo();
        }

        private static void DumpManagedDrives()
        {
            Console.WriteLine(Environment.NewLine + "DRIVES");
            var drives = DriveInfo.GetDrives();
            ConsoleTable report = new ConsoleTable("Path", "", "-Free", "-Total", "Format");
            foreach (var di in drives)
            {
                try
                {
                    report.AddRow(di.RootDirectory.FullName,
                        di.IsReady ? "OK" : "--",
                        Formatter.FormatBytes(di.AvailableFreeSpace),
                        Formatter.FormatBytes(di.TotalSize),
                        di.DriveType + " " + di.DriveFormat
                    );
                }
                catch (Exception ex)
                {
                    report.AddRow(
                        di.RootDirectory.FullName,
                        di.IsReady ? "OK" : "--",
                        "",
                        "",
                        ex.GetType().Name + " " + ex.Message.Replace(Environment.NewLine, " "));
                }
            }

            Console.WriteLine(report.ToString());
        }

        static void DumpUnixDriveInfo()
        {
            bool isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var mounts = ProcMountsParser.Parse(isWin ? "mounts" : "/proc/mounts").Entries;

            foreach (var mount in mounts)
            {
                try
                {
                    UnixDriveInfo di = new UnixDriveInfo(mount.MountPath);
                    var diInfo = new
                    {
                        di.Name,
                        di.AvailableFreeSpace,
                        di.DriveFormat,
                        di.IsReady,
                        di.DriveType,
                        di.MaximumFilenameLength,
                        RootDirectory = di.RootDirectory.FullName,
                        di.TotalFreeSpace,
                        di.TotalSize,
                        di.VolumeLabel,
                    };
                    DebugDumper.Trace($"UnixDriveInfo.GetForSpecialFile for {mount}{Environment.NewLine}{diInfo.AsJson()}{Environment.NewLine}");
                }
                catch (Exception ex)
                {
                    DebugDumper.Trace($"FAILED UnixDriveInfo.GetForSpecialFile for {mount.MountPath}{Environment.NewLine}{ex}{Environment.NewLine}");
                }
            }
        }

        private static void DumpProcMounts()
        {
            bool isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            IList<MountEntry> mounts = ProcMountsParser.Parse(isWin ? "mounts" : "/proc/mounts").Entries;

            ProcMountsAnalyzer analyz = ProcMountsAnalyzer.Create(mounts);
            Console.WriteLine(analyz.RawDetailsLog);

            DebugDumper.Dump(analyz, "ProcMountsAnalyzer.js");

        }

        private static void DumpProcMounts_Obsolete()
        {
            bool isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            IList<MountEntry> mounts = ProcMountsParser.Parse(isWin ? "mounts" : "/proc/mounts").Entries;
                ConsoleTable report = new ConsoleTable(
                    "Device", "FS", "Path", // from /proc/mounts
                    "", "-Free", "-Total", "Label", "msec",""
                );

                foreach (var mount in mounts)
                {
                    Exception error = null;
                    string driveInfo = null;
                    Stopwatch sw = Stopwatch.StartNew();
                    DriveDetails details = null;
                    try
                    {
                        // TRY System.IO.DriveInfo
                        var di = new DriveInfo(mount.MountPath);
                        details = new DriveDetails()
                        {
                            MountEntry = mount,
                            IsReady = di.IsReady,
                            Format = di.DriveType.ToString(),
                            FreeSpace = di.TotalFreeSpace,
                            TotalSize = di.TotalSize,
                        };

                    }
                    catch (Exception exNet)
                    {
                        error = exNet;
                        // TRY UnixDriveInfo 
                        try
                        {
                            UnixDriveInfo di = new UnixDriveInfo(mount.MountPath);
                            details = new DriveDetails()
                            {
                                MountEntry = mount,
                                IsReady = di.IsReady && di.TotalSize > 0,
                                Format = di.DriveType.ToString(),
                                FreeSpace = di.TotalFreeSpace,
                                TotalSize = di.TotalSize,
                            };
                            error = null;
                        }
                        catch (Exception exUnix)
                        {
                            // error = exUnix;
                        }
                    }

                    double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                    if (details != null)
                    {
                        report.AddRow(
                            mount.Device, mount.FileSystem, mount.MountPath,
                            details.IsReady ? "OK" : "--", Formatter.FormatBytes(details.FreeSpace), Formatter.FormatBytes(details.TotalSize),
                            details.Format, $"{msec:f2}"
                        );
                    }
                    else if (error != null)
                    {
                        string errorInfo = error.GetType().Name + ": " + error.Message.Replace(Environment.NewLine, " ");
                        report.AddRow(
                            mount.Device, mount.FileSystem, mount.MountPath,
                            "--", "", "",
                            "", $"{msec:f2}", errorInfo
                        );
                    }

                }

                Console.WriteLine(report);
        }

        static void DumpUnixDrives()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) return;

            var drives = UnixDriveInfo.GetDrives();

            List<dynamic> objs = new List<dynamic>();
            foreach (var i in drives)
            {
                objs.Add(new
                {
                    i.Name,
                    i.AvailableFreeSpace,
                    i.DriveFormat,
                    i.IsReady,
                    i.DriveType,
                    i.MaximumFilenameLength,
                    RootDirectory = i.RootDirectory.FullName,
                    i.TotalFreeSpace,
                    i.TotalSize,
                    i.VolumeLabel,
                });
            }
            DebugDumper.Dump(objs, "UnixDriveInfo.GetDrives.js");
        }
    }
}
