using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Microsoft.Extensions.Logging;

namespace Universe.Dashboard.Agent
{
   
    public class NetStatTimer
    {
        private static readonly UTF8Encoding Utf8Encoding = new UTF8Encoding(false);

        public static void Process()
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            var prevNetStat = new RawNetStatReader(new StringReader(GetRawNetStat())).NetStatItems;
            var prevTicks = sw.ElapsedTicks;

            var prevNetDev = GetNetDevInterfaces();
            
            PreciseTimer.AddListener("NetStat::Timer", () =>
            {
                List<NetStatRow> nextNetStat = new RawNetStatReader(new StringReader(GetRawNetStat())).NetStatItems;
                IList<NetDevInterfaceRow> nextNetDev = GetNetDevInterfaces();
                var nextTicks = sw.ElapsedTicks;
                var at = DateTime.UtcNow;

                double duration = (nextTicks - prevTicks) * 1d / Stopwatch.Frequency;
                
                // Total Received/Send
                var currentNetStat = new List<NetStatRow>();
                for (int i = 0; i < prevNetStat.Count && i < nextNetStat.Count; i++)
                {
                    if (prevNetStat[i].Group == nextNetStat[i].Group && prevNetStat[i].Key == nextNetStat[i].Key)
                    {
                        currentNetStat.Add(new NetStatRow() { Group = prevNetStat[i].Group, Key = prevNetStat[i].Key, Long = nextNetStat[i].Long - prevNetStat[i].Long});
                    }
                }

                var ipExtList = currentNetStat.Where(x => x.Group == "IpExt").ToList();
                var inOctetsRow = ipExtList.FirstOrDefault(x => x.Key == "InOctets"); 
                var outOctetsRow = ipExtList.FirstOrDefault(x => x.Key == "OutOctets");
                long currentInOctects = inOctetsRow?.Long ?? 0; 
                long currentOutOctects = outOctetsRow?.Long ?? 0;
                // Total received
                long currentInOctectsPerSec = (long)(currentInOctects / duration); 
                // Total sent
                long currentOutOctectsPerSec = (long)(currentOutOctects / duration);
                
                // Net/Dev interfaces
                var currentNetDev = new List<NetDevInterfaceRow>();
                foreach (var nextInterface in nextNetDev)
                {
                    // null below means newly created interface
                    var prevInterface = prevNetDev.FirstOrDefault(x => x.Name == nextInterface.Name) ??
                                        new NetDevInterfaceRow();

                    NetDevInterfaceRow deltaInterface = nextInterface - prevInterface;
                    currentNetDev.Add(deltaInterface);
                }

                NetStatDataSourcePoint point = new NetStatDataSourcePoint()
                {
                    At = at,
                    InterfacesStat = currentNetDev,
                    RxTotalOctets = currentInOctectsPerSec,
                    TxTotalOctets = currentOutOctectsPerSec,
                };

                var logBy1Seconds = NetStatDataSource.Instance.By_1_Seconds;
                while (logBy1Seconds.Count >= 60)
                    logBy1Seconds.RemoveAt(0);
                
                logBy1Seconds.Add(point);
                Dump_By_1_Seconds();

                prevNetStat = nextNetStat;
                prevNetDev = nextNetDev;
                prevTicks = nextTicks;
            });
        }

        [Conditional("DUMPS")]
        static void Dump_By_1_Seconds()
        {
            var copy = NetStatDataSource.Instance.By_1_Seconds;
            var viewModel = NetDataSourceView.AsViewModel(copy);
            DebugDumper.Dump(NetStatDataSource.Instance.By_1_Seconds, "NetStatDataSource.1s.json");
            DebugDumper.Dump(NetStatDataSource.Instance.By_1_Seconds, "NetStatDataSource.1s.min.json", true);
            DebugDumper.Dump(viewModel, "NetStat.ViewModel.1s.json");
            DebugDumper.Dump(viewModel, "NetStat.ViewModel.1s.min.json", true);
        }
        
        static string GetRawNetStat()
        {
            var file = "/proc/net/netstat";
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rdr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                return rdr.ReadToEnd();
            }
        }

        private static IList<NetDevInterfaceRow> GetNetDevInterfaces()
        {
            using(FileStream fs = new FileStream("/proc/net/dev", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader rdr = new StreamReader(fs, Utf8Encoding))
                {
                    NetDevParser netDevParser = new NetDevParser(rdr);
                    return netDevParser.Interfaces;
                }
            }
        }
   }
}