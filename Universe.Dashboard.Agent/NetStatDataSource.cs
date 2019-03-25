using System.Collections.Generic;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.Agent
{
    public class NetStatDataSource
    {
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

    public class NetStatDataSourcePersistence
    {
        public static void Flush()
        {
            DashboardContext db = new DashboardContext();
            HistoryLogic history = new HistoryLogic(db);
            // It is NOT thread safe
            history.Save("NetStatDataSource.By_1_Seconds", NetStatDataSource.Instance.By_1_Seconds);
            history.Save("NetStatDataSource", NetStatDataSource.Instance);
        }
    }
}
