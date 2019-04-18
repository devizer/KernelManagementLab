using System;
using System.Collections.Generic;
using KernelManagementJam;

namespace Universe.Dashboard.Agent
{
    public class BlockDiskDataSource
    {
        public Dictionary<string,BlockStatistics> TotalBlockDisks { get; set; }
        
        public List<BlockDiskDataSourcePoint> By_1_Seconds;
        public List<BlockDiskDataSourcePoint> By_5_Seconds;
        public List<BlockDiskDataSourcePoint> By_15_Seconds;
        public List<BlockDiskDataSourcePoint> By_30_Seconds;
        public List<BlockDiskDataSourcePoint> By_60_Seconds;
        
        public static readonly BlockDiskDataSource Instance = new BlockDiskDataSource()
        {
            By_1_Seconds = new List<BlockDiskDataSourcePoint>(),
            By_5_Seconds = new List<BlockDiskDataSourcePoint>(),
            By_15_Seconds = new List<BlockDiskDataSourcePoint>(),
            By_30_Seconds = new List<BlockDiskDataSourcePoint>(),
            By_60_Seconds = new List<BlockDiskDataSourcePoint>(),
        };

    }
    
    public class BlockDiskDataSourcePoint
    {
        public DateTime At { get; set; }
        
        public List<DiskVolStatModel> BlockDiskStat { get; set; }
    }

    public class DiskVolStatModel
    {
        // disk | volume
        public string Kind { get; set; }
        public string DiskVolKey { get; set; }
        public BlockStatistics Stat { get; set; }
    }

}
