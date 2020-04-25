using System;
using System.Collections.Generic;
using KernelManagementJam;

namespace Universe.Dashboard.Agent
{
    public class MemorySummaryDataSource
    {
        public List<MemorySummaryDataSourcePoint> By_1_Seconds;
        public List<MemorySummaryDataSourcePoint> By_5_Seconds;
        public List<MemorySummaryDataSourcePoint> By_15_Seconds;
        public List<MemorySummaryDataSourcePoint> By_30_Seconds;
        public List<MemorySummaryDataSourcePoint> By_60_Seconds;
        
        public static readonly MemorySummaryDataSource Instance = new MemorySummaryDataSource()
        {
            By_1_Seconds = new List<MemorySummaryDataSourcePoint>(),
            By_5_Seconds = new List<MemorySummaryDataSourcePoint>(),
            By_15_Seconds = new List<MemorySummaryDataSourcePoint>(),
            By_30_Seconds = new List<MemorySummaryDataSourcePoint>(),
            By_60_Seconds = new List<MemorySummaryDataSourcePoint>(),
        };

    }

    public class MemorySummaryDataSourcePoint
    {
        public DateTime At { get; set; }
        public LinuxMemorySummary Summary { get; set; }
    }
}