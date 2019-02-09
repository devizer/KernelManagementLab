﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinuxNetStatLab;

namespace MountLab
{
    class Program
    {
        static void Main(string[] args)
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
                    driveInfo = string.Format("IsReady: {0}, Label: [{3}], Free: {1} / {2}, ",
                        di.IsReady, di.AvailableFreeSpace, di.TotalSize, di.VolumeLabel);

                    double msec = sw.ElapsedTicks / (double) Stopwatch.Frequency;
                    driveInfo = string.Format("[{0:0.00} msec] ", msec) + driveInfo;
                }
                catch (Exception ex)
                {
                    double msec = sw.ElapsedTicks / (double)Stopwatch.Frequency;
                    driveInfo = string.Format("[{0:0.00} msec] ", msec) + ex.GetType().Name + ": " + ex.Message;
                }

                Console.WriteLine(mount + " --> " + driveInfo);


            }
        }


    }
}
