using System;
using System.Collections.Generic;
using System.IO;
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
