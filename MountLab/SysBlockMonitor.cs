using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KernelManagementJam;

namespace MountLab
{
    class SysBlockMonitor
    {
        public static void Run()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var prev = SysBlocksReader.GetSnapshot();
            var prevTicks = sw.ElapsedTicks;

            while (true)
            {
                Thread.Sleep(1);
            }
        }
    }
}
