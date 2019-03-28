using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using KernelManagementJam;
using Microsoft.AspNetCore.Authorization;

namespace Universe.Dashboard.Agent
{
    public class NetDataSourceView
    {

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

            // InterfaceName, FieldName, Y[]  
            Dictionary<string, Dictionary<string, List<long>>> interfacesView = new Dictionary<string, Dictionary<string, List<long>>>();
            int atPosition = 0;
            foreach (var atStat in dataSource)
            {
                atPosition++;
                foreach (var interfaceRow in atStat.InterfacesStat)
                {
                    var interfaceName = interfaceRow.Name;
                    var byInterface = interfacesView.GetOrAdd(interfaceName, _ => new Dictionary<string, List<long>>());
                    foreach (var fieldMetadata in fields)
                    {
                        string fieldName = fieldMetadata.Field;
                        var byField = byInterface.GetOrAdd(fieldName, _ => new List<long>());
                        long fieldValue = fieldMetadata.GetField(interfaceRow);
                        byField.Add(fieldValue);
                    }
                }

                var missedInterfaces = interfaceNameList.Except(atStat.InterfacesStat.Select(x => x.Name));
                foreach (var missedInterface in missedInterfaces)
                {
                    var byInterface = interfacesView.GetOrAdd(missedInterface, _ => new Dictionary<string, List<long>>());
                    foreach (var fieldMetadata in fields)
                    {
                        string fieldName = fieldMetadata.Field;
                        var byField = byInterface.GetOrAdd(fieldName, _ => new List<long>());
                        long fieldValue = byField.LastOrDefault();
                        byField.Add(fieldValue);
                    }
                }
            }

            ret.Interfaces = interfacesView;
            return ret;
        }
    }
}