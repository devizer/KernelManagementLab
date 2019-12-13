using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using KernelManagementJam;

namespace Universe.Dashboard.Agent
{
    public class NetDataSourceView
    {

        public static List<string> GetInterfaceNames()
        {
            
            var totals = NetStatDataSource.Instance.TotalsOfInterfaces;
            if (totals == null) return null;
            return totals
                .Select(x => new {Name = x.Key, Sum = x.Value.RxBytes + x.Value.TxBytes})
                .OrderByDescending(x => x.Sum)
                .Select(x => x.Name)
                .ToList();
        }

        public static object AsViewModel(List<NetStatDataSourcePoint> dataSource)
        {
            dynamic ret = new ExpandoObject();
            List<string> interfaceNameList = dataSource
                .SelectMany(x => x.InterfacesStat)
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            
            IDictionary<string, object> d = (IDictionary<string,object>)ret;
            if (dataSource.Any())
            {
                ret.XRange = new {Min = dataSource.First().At, Max = dataSource.Last().At};
            }
            else
            {
                var now = DateTime.UtcNow;
                ret.XRange = new {Min = now - TimeSpan.FromMinutes(1), Max = now};
            }
            
            ret.InterfaceNames = interfaceNameList;
            var xAxisValues = dataSource.Select(x => x.At).ToList();
            ret.X = xAxisValues;
            ret.Summary = new
            {
                RxTotalOctects = dataSource.Select(x => x.RxTotalOctets),
                TxTotalOctects = dataSource.Select(x => x.TxTotalOctets),
            };

            
            var fields = new[]
            {
                new {Field = "RxBytes", GetField = (Func<NetDevInterfaceRow,long>) (row => row.RxBytes)},
                new {Field = "RxPackets", GetField = (Func<NetDevInterfaceRow,long>) (row => row.RxPackets)},
                new {Field = "RxErrors", GetField = (Func<NetDevInterfaceRow,long>) (row => row.RxErrors)},
                new {Field = "RxDrops", GetField = (Func<NetDevInterfaceRow,long>) (row => row.RxDrops)},
                new {Field = "RxFifoErrors", GetField = (Func<NetDevInterfaceRow,long>) (row => row.RxFifoErrors)},
                new {Field = "RxFrameErrors", GetField = (Func<NetDevInterfaceRow,long>) (row => row.RxFrameErrors)},
                new {Field = "RxCompressed", GetField = (Func<NetDevInterfaceRow,long>) (row => row.RxCompressed)},
                new {Field = "Multicast", GetField = (Func<NetDevInterfaceRow,long>) (row => row.Multicast)},
                
                new {Field = "TxBytes", GetField = (Func<NetDevInterfaceRow,long>) (row => row.TxBytes)},
                new {Field = "TxPackets", GetField = (Func<NetDevInterfaceRow,long>) (row => row.TxPackets)},
                new {Field = "TxErrors", GetField = (Func<NetDevInterfaceRow,long>) (row => row.TxErrors)},
                new {Field = "TxDrops", GetField = (Func<NetDevInterfaceRow,long>) (row => row.TxDrops)},
                new {Field = "TxFifoErrors", GetField = (Func<NetDevInterfaceRow,long>) (row => row.TxFifoErrors)},
                new {Field = "Collisions", GetField = (Func<NetDevInterfaceRow,long>) (row => row.Collisions)},
                new {Field = "TxHeartbeatErrors", GetField = (Func<NetDevInterfaceRow,long>) (row => row.TxHeartbeatErrors)},
                new {Field = "TxCompressed", GetField = (Func<NetDevInterfaceRow,long>) (row => row.TxCompressed)},
            };

            // interfaceName: PublicFastInterfaceMetrics
            Dictionary<string, PublicFastInterfaceMetrics> fastInterfacesView = new Dictionary<string, PublicFastInterfaceMetrics>();

            foreach (var atStat in dataSource)
            {
                foreach (var interfaceRow in atStat.InterfacesStat)
                {
                    var interfaceName = interfaceRow.Name;
                    var publicFastInterfaceMetrics = fastInterfacesView.GetOrAdd(interfaceName, _ => new PublicFastInterfaceMetrics());
                    publicFastInterfaceMetrics.Append(interfaceRow);
/*
                    var byInterface = interfacesView.GetOrAdd(interfaceName, _ => new Dictionary<string, List<long>>());
                    foreach (var fieldMetadata in fields)
                    {
                        string fieldName = fieldMetadata.Field;
                        var byField = byInterface.GetOrAdd(fieldName, _ => new List<long>());
                        long fieldValue = fieldMetadata.GetField(interfaceRow);
                        byField.Add(fieldValue);
                    }
*/
                }

                var missedInterfaces = interfaceNameList.Except(atStat.InterfacesStat.Select(x => x.Name));
                foreach (var missedInterface in missedInterfaces)
                {
                    var publicFastBlockMetrics = fastInterfacesView.GetOrAdd(missedInterface, _ => new PublicFastInterfaceMetrics());
                    publicFastBlockMetrics.AppendMissed();

/*
                    var byInterface = interfacesView.GetOrAdd(missedInterface, _ => new Dictionary<string, List<long>>());
                    foreach (var fieldMetadata in fields)
                    {
                        string fieldName = fieldMetadata.Field;
                        var byField = byInterface.GetOrAdd(fieldName, _ => new List<long>());
                        long fieldValue = byField.LastOrDefault();
                        byField.Add(fieldValue);
                    }
*/
                }
            }
            
            // InterfaceName, FieldName, Y[]  
            Dictionary<string, Dictionary<string, List<long>>> interfacesView = new Dictionary<string, Dictionary<string, List<long>>>();
            foreach (var fastPairs in fastInterfacesView)
            {
                var interfaceName = fastPairs.Key;
                var publicFastInterfaceMetrics = fastPairs.Value;
                interfacesView[interfaceName] = publicFastInterfaceMetrics.AsPublicView();
            }

            
            // Inject IsInactive
            var totals = NetStatDataSource.Instance.TotalsOfInterfaces;
//            foreach (var interfacePair in interfacesView)
//            {
//                if (!totals.TryGetValue(interfacePair.Key, out var t))
//                interfacePair.Value["IsInactive"] =   
//            }

            ret.Interfaces = interfacesView;
            ret.InterfaceTotals = NetStatDataSource.Instance.TotalsOfInterfaces;
            return ret;
        }
        
        class PublicFastInterfaceMetrics
        {
            public List<long> RxBytes = new List<long>(61);
            public List<long> RxPackets = new List<long>(61);
            public List<long> RxErrors = new List<long>(61);
            public List<long> RxDrops = new List<long>(61);
            public List<long> RxFifoErrors = new List<long>(61);
            public List<long> RxFrameErrors = new List<long>(61);
            public List<long> RxCompressed = new List<long>(61);
            public List<long> Multicast = new List<long>(61);

            public List<long> TxBytes = new List<long>(61);
            public List<long> TxPackets = new List<long>(61);
            public List<long> TxErrors = new List<long>(61);
            public List<long> TxDrops = new List<long>(61);
            public List<long> TxFifoErrors = new List<long>(61);
            public List<long> Collisions = new List<long>(61);
            public List<long> TxHeartbeatErrors = new List<long>(61);
            public List<long> TxCompressed = new List<long>(61);

            public void Append(NetDevInterfaceRow blockRow)
            {
                RxBytes.Add(blockRow.RxBytes);
                RxPackets.Add(blockRow.RxPackets);
                RxErrors.Add(blockRow.RxErrors);
                RxDrops.Add(blockRow.RxDrops);
                RxFifoErrors.Add(blockRow.RxFifoErrors);
                RxFrameErrors.Add(blockRow.RxFrameErrors);
                RxCompressed.Add(blockRow.RxCompressed);
                Multicast.Add(blockRow.Multicast);
                
                TxBytes.Add(blockRow.TxBytes);
                TxPackets.Add(blockRow.TxPackets);
                TxErrors.Add(blockRow.TxErrors);
                TxDrops.Add(blockRow.TxDrops);
                TxFifoErrors.Add(blockRow.TxFifoErrors);
                Collisions.Add(blockRow.Collisions);
                TxHeartbeatErrors.Add(blockRow.TxHeartbeatErrors);
                TxCompressed.Add(blockRow.TxCompressed);

            }

            public void AppendMissed()
            {
                RxBytes.Add(RxBytes.LastOrDefault());
                RxPackets.Add(RxPackets.LastOrDefault());
                RxErrors.Add(RxErrors.LastOrDefault());
                RxDrops.Add(RxDrops.LastOrDefault());
                RxFifoErrors.Add(RxFifoErrors.LastOrDefault());
                RxFrameErrors.Add(RxFrameErrors.LastOrDefault());
                RxCompressed.Add(RxCompressed.LastOrDefault());
                Multicast.Add(Multicast.LastOrDefault());
                
                TxBytes.Add(TxBytes.LastOrDefault());
                TxPackets.Add(TxPackets.LastOrDefault());
                TxErrors.Add(TxErrors.LastOrDefault());
                TxDrops.Add(TxDrops.LastOrDefault());
                TxFifoErrors.Add(TxFifoErrors.LastOrDefault());
                Collisions.Add(Collisions.LastOrDefault());
                TxHeartbeatErrors.Add(TxHeartbeatErrors.LastOrDefault());
                TxCompressed.Add(TxCompressed.LastOrDefault());

            }

            public Dictionary<string, List<long>> AsPublicView()
            {
                Dictionary<string, List<long>> ret = new Dictionary<string, List<long>>();
                ret["RxBytes"] = this.RxBytes;
                ret["RxPackets"] = this.RxPackets;
                ret["RxErrors"] = this.RxErrors;
                ret["RxDrops"] = this.RxDrops;
                ret["RxFifoErrors"] = this.RxFifoErrors;
                ret["RxFrameErrors"] = this.RxFrameErrors;
                ret["RxCompressed"] = this.RxCompressed;
                ret["Multicast"] = this.Multicast;
                
                ret["TxBytes"] = this.TxBytes;
                ret["TxPackets"] = this.TxPackets;
                ret["TxErrors"] = this.TxErrors;
                ret["TxDrops"] = this.TxDrops;
                ret["TxFifoErrors"] = this.TxFifoErrors;
                ret["Collisions"] = this.Collisions;
                ret["TxHeartbeatErrors"] = this.TxHeartbeatErrors;
                ret["TxCompressed"] = this.TxCompressed;

                return ret;
            }
        }

    }
}