using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConsoleTables;
using KernelManagementJam;
using LinuxNetStatLab;
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
        }

        private static void DumpManagedDrives()
        {
            Console.WriteLine(Environment.NewLine + "DRIVES");
            var drives = DriveInfo.GetDrives();
            foreach (var di in drives)
            {
                string driveInfo;
                try
                {
                    driveInfo = string.Format("{0,-21} [{3,-12}], Free: {1,-12} of {2,-12}, Fmt: {4,-12}",
                        di.IsReady, Formatter.FormatBytes(di.AvailableFreeSpace), Formatter.FormatBytes(di.TotalSize), di.VolumeLabel,
                        di.DriveFormat);
                }
                catch (Exception ex)
                {
                    driveInfo = ex.GetType().Name + ": " + ex.Message.Replace(Environment.NewLine, " ");
                }

                Console.WriteLine(di.RootDirectory + " --> " + driveInfo);
            }
        }

        private static void DumpProcMounts_Wrong()
        {
            bool isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var mounts = ProcMountsParser.Parse(isWin ? "mounts" : "/proc/mounts").Entries;
            List<dynamic> report = new List<dynamic>();
            foreach (var mount in mounts)
            {
                string driveInfo = null;
                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    var di = new DriveInfo(mount.MountPath);
                    double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;

                    report.Add(new
                    {
                        mount.Device,
                        mount.MountPath,
                        mount.FileSystem,
                        IsReady = di.IsReady + string.Format(", {0:0.00} msec", msec),
                        di.DriveFormat,
                        Free = Formatter.FormatBytes(di.AvailableFreeSpace),
                        Total = Formatter.FormatBytes(di.TotalSize),
                    });
                }
                catch (Exception ex)
                {
                    double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                    report.Add(new
                    {
                        mount.Device,
                        mount.MountPath,
                        mount.FileSystem,
                        IsReady = ex.GetType().Name,
                    });

                }

            }

            ConsoleTable.From(report).Write(Format.Alternative);
        }

        private static void DumpProcMounts()
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
                    DebugDumper.Trace($"UnixDriveInfo.GetForSpecialFile for {mount}{Environment.NewLine}{diInfo.AsJson()}");
                }
                catch (Exception ex)
                {
                    DebugDumper.Trace($"FAILED UnixDriveInfo.GetForSpecialFile for {mount.MountPath}{Environment.NewLine}{ex}");
                }

                string driveInfo = null;
                Stopwatch sw = Stopwatch.StartNew();
                var mountInfo = string.Format("{0,-23} | {1,-12} | {2,-31} | ", mount.Device, mount.FileSystem, mount.MountPath);
                try
                {
                    var di = new DriveInfo(mount.MountPath);
                    driveInfo = string.Format("{0,2} | {1,9} | {2,9} | {3,7} | {4,15}",
                        di.IsReady == true ? "OK" : "--",
                        Formatter.FormatBytes(di.AvailableFreeSpace),
                        Formatter.FormatBytes(di.TotalSize),
                        di.DriveFormat, di.VolumeLabel
                    );

                    double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                    driveInfo = string.Format("[{0:0.00} msec] ", msec) + driveInfo;
                }
                catch (Exception ex)
                {
                    double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                    driveInfo = string.Format("[{0:0.00} msec] ", msec) + ex.GetType().Name + ": " + ex.Message.Replace(Environment.NewLine, " ");
                }

                Console.WriteLine(mountInfo + "" + driveInfo);
            }
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
