using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Mono.Unix;

namespace MountLab
{
    class Program
    {
        static void Main(string[] args)
        {
            PathNormalizationLab();


            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                var blockDevices = SysBlocksReader.GetSnapshot();
                DebugDumper.Dump(blockDevices, "debug-dumps/SysBlockSnapshot.js");
            }

            DumpProcMounts();
            DumpManagedDrives();
            DumpUnixDrives();
            DumpUnixDriveInfo();

            if ("Monitor-V1".Equals(args.FirstOrDefault(), StringComparison.InvariantCultureIgnoreCase))
                SysBlockMonitorV1.RunMonV1();
        }

        private static void PathNormalizationLab()
        {

            Func<string, string> asCurrentPath = x => x.Replace("/", Path.DirectorySeparatorChar.ToString());
            var path1 = Path.Combine(asCurrentPath("/one/two/three"), asCurrentPath("../target"));
            var path2 = Path.Combine(asCurrentPath("/one/two/three"), asCurrentPath("/target"));
            DebugDumper.Trace($"path1: [{path1}], path2: [{path2}]");
            var path1abs = new DirectoryInfo(path1).FullName;
            var path2abs = new DirectoryInfo(path2).FullName;
            DebugDumper.Trace($"path1 abs: [{path1abs}], path2 abs: [{path2abs}]");
        }

        private static void DumpManagedDrives()
        {
            Console.WriteLine(Environment.NewLine + "DRIVES");
            Stopwatch sw = Stopwatch.StartNew();
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
            Console.WriteLine($"DriveInfo.GetDrives() took {sw.Elapsed}");
            
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
            ProcMountsSandbox.DumpProcMounts();
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
            DebugDumper.Dump(objs, "debug-dumps/UnixDriveInfo.GetDrives.js");
        }
    }
}
