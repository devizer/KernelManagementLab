using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;

namespace LinuxNetStatLab
{
    public class NetDevLab
    {
        public static double JustParse()
        {
            IList<NetDevInterfaceRow> result;
            using(FileStream fs = new FileStream("/proc/net/dev", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rdr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                Stopwatch sw = Stopwatch.StartNew();
                NetDevParser netDevParser = new NetDevParser(rdr);
                result = netDevParser.Interfaces;
                double msec = sw.ElapsedTicks / (double) Stopwatch.Frequency;
                DebugDumper.Dump(result, "Interfaces.json");
                return msec;
            }
        }
    }
}