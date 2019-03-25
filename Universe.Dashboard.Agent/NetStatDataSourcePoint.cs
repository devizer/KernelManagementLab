using System;
using System.Collections.Generic;
using KernelManagementJam;

namespace Universe.Dashboard.Agent
{
    public class NetStatDataSourcePoint
    {
        public DateTime At { get; set; }
        public long RxTotalOctets { get; set; }
        public long TxTotalOctets { get; set; }
        
        public List<NetDevInterfaceRow> InterfacesStat { get; set; }
    }
    
}
