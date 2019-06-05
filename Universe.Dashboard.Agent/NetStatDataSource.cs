using System.Collections.Generic;
using KernelManagementJam;

namespace Universe.Dashboard.Agent
{
    public class NetStatDataSource
    {
        public Dictionary<string,NetDevInterfaceRow> TotalsOfInterfaces = null;
        
        // 60 * 1
        public List<NetStatDataSourcePoint> By_1_Seconds;
        
        // 60 * 5
        public List<NetStatDataSourcePoint> By_5_Seconds;
        
        // 60 * 15
        public List<NetStatDataSourcePoint> By_15_Seconds;

        // 60 * 30
        public List<NetStatDataSourcePoint> By_30_Seconds;
        
        // 60 * 60
        public List<NetStatDataSourcePoint> By_60_Seconds;

        public static readonly NetStatDataSource Instance = new NetStatDataSource()
        {
            By_1_Seconds = new List<NetStatDataSourcePoint>(),
            By_5_Seconds = new List<NetStatDataSourcePoint>(),
            By_15_Seconds = new List<NetStatDataSourcePoint>(),
            By_30_Seconds = new List<NetStatDataSourcePoint>(),
            By_60_Seconds = new List<NetStatDataSourcePoint>(),
        };

    }
}
