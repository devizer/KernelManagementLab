using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelManagementJam;
using LinuxNetStatLab;
using Mono.Unix;
using Newtonsoft.Json;

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
                    driveInfo = string.Format("IsReady: {0}, Label: [{3}], Free: {1} / {2}, Fmt: {4}",
                        di.IsReady, Formatter.FormatBytes(di.AvailableFreeSpace), Formatter.FormatBytes(di.TotalSize), di.VolumeLabel,
                        di.DriveFormat);
                }
                catch (Exception ex)
                {
                    driveInfo = ex.GetType().Name + ": " + ex.Message;
                }

                Console.WriteLine(di.RootDirectory + " --> " + driveInfo);
            }
        }

        private static void DumpProcMounts()
        {
            bool isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var mounts = ProcMountsParser.Parse(isWin ? "mounts" : "/proc/mounts").Entries;
            foreach (var mount in mounts)
            {
                string driveInfo = null;
                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    var di = new DriveInfo(mount.MountPath);
                    driveInfo = string.Format("IsReady: {0}, Label: [{3}], Free: {1} / {2}, Fmt: {4}",
                        di.IsReady, Formatter.FormatBytes(di.AvailableFreeSpace), Formatter.FormatBytes(di.TotalSize), di.VolumeLabel,
                        di.DriveFormat);

                    double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                    driveInfo = string.Format("[{0:0.00} msec] ", msec) + driveInfo;
                }
                catch (Exception ex)
                {
                    double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                    driveInfo = string.Format("[{0:0.00} msec] ", msec) + ex.GetType().Name + ": " + ex.Message;
                }

                Console.WriteLine(mount + " --> " + driveInfo);
            }
        }

        static void DumpUnixDrives()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) return;

            var drives = UnixDriveInfo.GetDrives();

            foreach (var unixDriveInfo in drives)
            {
                DebugDumper.Dump(drives, "UnixDriveInfo.GetDrives.js");
            }
        }




    }

    class DebugDumper
    {
        public static void Dump(object anObject, string fileName)
        {

            JsonSerializer ser = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            StringBuilder json = new StringBuilder();
            StringWriter jwr = new StringWriter(json);
            ser.Serialize(jwr, anObject);
            jwr.Flush();

            // string json = JsonConvert.SerializeObject(anObject, Formatting.Indented, settings);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                wr.Write(json);
            }
        }
    }

    static class Formatter
    {
        public static string FormatBytes(long number)
        {
            if (number == 0)
                return "0";
            else if (number < 9999)
                return number.ToString("n0") + "B";
            else if (number < 9999999)
                return (number / 1024d).ToString("n1") + "K";
            else if (number < 9999999999)
                return (number / 1024d / 1024d).ToString("n1") + "M";
            else 
                return (number / 1024d / 1024d / 1024d).ToString("n1") + "G";
        }
    }
}
