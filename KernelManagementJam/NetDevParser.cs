using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;

namespace KernelManagementJam
{
    // https://stackoverflow.com/questions/3521678/what-are-meanings-of-fields-in-proc-net-dev
    public class NetDevParser
    {
        public IList<NetDevInterfaceRow> Interfaces { get; }
        public TextReader Arg { get; }

        public NetDevParser(TextReader arg)
        {
            Arg = arg;
            Interfaces = new List<NetDevInterfaceRow>(Parse());
        }

        IEnumerable<NetDevInterfaceRow> Parse()
        {
            while (true)
            {
                string line = Arg.ReadLine();
                if (line == null) break;
                
                // line should start from '{name}:'
                var arrNameAndColumns = line.Split(':');
                if (arrNameAndColumns.Length < 2) continue;
                var name = arrNameAndColumns[0].Trim();
                var rawWholeColumnsAsString = arrNameAndColumns[1].Trim();
                
                // name is not empty
                if (string.IsNullOrEmpty(name)) continue;
                
                // counters count should be 16 (or above?) 
                var arrColumnsAsStrings = rawWholeColumnsAsString.Split(' ').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
                var columnsCount = arrColumnsAsStrings.Length;
                if (columnsCount < 16) continue;
                
                // all the counters should be valid numbers
                var columnsAsLongs = new long[columnsCount];
                for (int i = 0; i < columnsCount && i < 16; i++)
                {
                    long longValue;
                    if (!long.TryParse(arrColumnsAsStrings[i], out longValue))
                        continue;

                    columnsAsLongs[i] = longValue;
                }

                yield return new NetDevInterfaceRow()
                {
                    Name  = name,                  // 0:
                                  
                    RxBytes  = columnsAsLongs[0],             // 1
                    RxPackets  = columnsAsLongs[1],           // 2
                    RxErrors  = columnsAsLongs[2],            // 3
                    RxDrops  = columnsAsLongs[3],             // 4
                    RxFifoErrors  = columnsAsLongs[4],        // 5
                    RxFrameErrors  = columnsAsLongs[5],       // 6
                    RxCompressed  = columnsAsLongs[6],        // 7   
                    Multicast  = columnsAsLongs[7],           // 8
                                  
                    TxBytes  = columnsAsLongs[8],             // 9
                    TxPackets  = columnsAsLongs[9],           // 10
                    TxErrors  = columnsAsLongs[10],           // 11
                    TxDrops  = columnsAsLongs[11],            // 12
                    TxFifoErrors  = columnsAsLongs[12],       // 13
                    Collisions  = columnsAsLongs[13],         // 14
                    TxHeartbeatErrors  = columnsAsLongs[14],  // 15
                    TxCompressed  = columnsAsLongs[15],       // 16 
                };
            }
        }
    }

    public class NetDevInterfaceRow
    {
        public string Name { get; set; }             // 0:
                                                     
        public long RxBytes { get; set; }            // 1
        public long RxPackets { get; set; }          // 2
        public long RxErrors { get; set; }           // 3
        public long RxDrops { get; set; }            // 4
        public long RxFifoErrors { get; set; }       // 5
        public long RxFrameErrors { get; set; }      // 6
        public long RxCompressed { get; set; }       // 7   
        public long Multicast { get; set; }          // 8
                                                     
        public long TxBytes { get; set; }            // 9
        public long TxPackets { get; set; }          // 10
        public long TxErrors { get; set; }           // 11
        public long TxDrops { get; set; }            // 12
        public long TxFifoErrors { get; set; }       // 13
        public long Collisions { get; set; }         // 14
        public long TxHeartbeatErrors { get; set; }  // 15
        public long TxCompressed { get; set; }       // 16

        public static NetDevInterfaceRow operator -(NetDevInterfaceRow next, NetDevInterfaceRow prev)
        {
            return Delta(next, prev);
        }

        public bool IsInactive =>
            RxBytes == 0 &&
            RxPackets == 0 &&
            RxErrors == 0 &&
            RxDrops == 0 &&
            RxFifoErrors == 0 &&
            RxFrameErrors == 0 &&
            RxCompressed == 0 &&
            Multicast == 0 &&

            TxBytes == 0 &&
            TxPackets == 0 &&
            TxErrors == 0 &&
            TxDrops == 0 &&
            TxFifoErrors == 0 &&
            Collisions == 0 &&
            TxHeartbeatErrors == 0 &&
            TxCompressed == 0;
        
        
        public static NetDevInterfaceRow Delta(NetDevInterfaceRow next, NetDevInterfaceRow prev)
        {
            return new NetDevInterfaceRow()
            {
                Name = next.Name,

                RxBytes = next.RxBytes - prev.RxBytes, // 1
                RxPackets = next.RxPackets - prev.RxPackets, // 2
                RxErrors = next.RxErrors - prev.RxErrors, // 3
                RxDrops = next.RxDrops - prev.RxDrops, // 4
                RxFifoErrors = next.RxFifoErrors - prev.RxFifoErrors, // 5
                RxFrameErrors = next.RxFrameErrors - prev.RxFrameErrors, // 6
                RxCompressed = next.RxCompressed - prev.RxCompressed, // 7   
                Multicast = next.Multicast - prev.Multicast, // 8

                TxBytes = next.TxBytes - prev.TxBytes, // 9
                TxPackets = next.TxPackets - prev.TxPackets, // 10
                TxErrors = next.TxErrors - prev.TxErrors, // 11
                TxDrops = next.TxDrops - prev.TxDrops, // 12
                TxFifoErrors = next.TxFifoErrors - prev.TxFifoErrors, // 13
                Collisions = next.Collisions - prev.Collisions, // 14
                TxHeartbeatErrors = next.TxHeartbeatErrors - prev.TxHeartbeatErrors, // 15
                TxCompressed = next.TxCompressed - prev.TxCompressed, // 16
            };

        }
        
    }
}