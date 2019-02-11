using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            ConsoleTable report = new ConsoleTable("Name", "Mount", "", "-Free", "-Total", "Format");
            foreach (var di in drives)
            {
                string driveInfo;
                try
                {
                    report.AddRow(di.Name, di.RootDirectory.FullName,
                        di.IsReady ? "OK" : "--",
                        Formatter.FormatBytes(di.AvailableFreeSpace),
                        Formatter.FormatBytes(di.TotalSize),
                        di.DriveType + " " + di.DriveFormat
                    );
                }
                catch (Exception ex)
                {
                    report.AddRow(
                        di.Name, 
                        di.RootDirectory.FullName,
                        di.IsReady ? "OK" : "--",
                        "",
                        "",
                        ex.GetType().Name + " " + ex.Message.Replace(Environment.NewLine, " "));
                }
            }

            Console.WriteLine(report.ToString());
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
                var mountInfo = string.Format("{0,-34} | {1,-17} | {2,-31} | ", mount.Device, mount.FileSystem, mount.MountPath);
                try
                {
                    var di = new DriveInfo(mount.MountPath);
                    driveInfo = string.Format("{0,2} | {1,9} | {2,9} | {3,-7} | {4,-15}",
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
