using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelManagementJam;
using LinuxNetStatLab;
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
                        di.IsReady, di.AvailableFreeSpace, di.TotalSize, di.VolumeLabel,
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
                        di.IsReady, di.AvailableFreeSpace, di.TotalSize, di.VolumeLabel,
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
    }

    class DebugDumper
    {
        public static void Dump(object anObject, string fileName)
        {
            string json = JsonConvert.SerializeObject(anObject, Formatting.Indented);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                wr.Write(json);
            }
        }
    }
}
