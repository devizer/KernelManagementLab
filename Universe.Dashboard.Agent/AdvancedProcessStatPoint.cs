using System;
using KernelManagementJam;
using Newtonsoft.Json;

namespace Universe.Dashboard.Agent
{
    // 
    public class AdvancedProcessStatPoint
    {
        
        [JsonIgnore]
        public ProcessIoStat Totals { get; private set; }
        
        [JsonIgnore]
        public LinuxTaskStats.LinuxTaskStats TaskStats { get; private set; }

        private bool IsTaskStatsActual => TaskStats.Version != 0;

        // Process
        public int Pid => Totals.Pid;
        public int ParentPid => Totals.ParentPid;
        public ProcessIoStat.ProcessKind Kind => Totals.Kind;
        public string User => string.IsNullOrEmpty(Totals.UserName) ? $"{Totals.Uid}" : $"{Totals.Uid}: {Totals.UserName}";
        public string Name => Totals.Name;
        public int MixedPriority => (int) Totals.MixedPriority; // Nice: 0...39 is -20..19. Realtime: -2 ==> 1, -3 ==> 2, ... -100 ==> 99
        public double Uptime { get; set; } // in seconds
        public int NumThreads => (int) Totals.NumThreads;
        public string CommandLine => Totals.Command;

        // Memory
        public long RSS => Totals.RssMem;
        public long Shared => Totals.SharedMem;
        public long Swapped => Totals.SwappedMem;

        // CPU Usage
        public double UserCpuUsage => Totals.UserCpuUsage; // seconds
        public double UserCpuUsage_PerCents { get; set; }
        public double KernelCpuUsage => Totals.KernelCpuUsage; // seconds
        public double KernelCpuUsage_PerCents { get; set; }
        public double TotalCpuUsage => (Totals.UserCpuUsage + Totals.KernelCpuUsage); // seconds
        public double TotalCpuUsage_PerCents { get; set; }

        // Children CPU Usage
        public double ChildrenUserCpuUsage => Totals.ChildrenUserCpuUsage; // seconds
        public double ChildrenUserCpuUsage_PerCents { get; set; }
        public double ChildrenKernelCpuUsage => Totals.ChildrenKernelCpuUsage; // seconds
        public double ChildrenKernelCpuUsage_PerCents { get; set; }
        public double ChildrenTotalCpuUsage => (Totals.ChildrenUserCpuUsage + Totals.ChildrenKernelCpuUsage); // seconds
        public double ChildrenTotalCpuUsage_PerCents { get; set; }
        
        // IO Time
        public double IoTime => IsTaskStatsActual ? (TaskStats.BlockIoDelay + TaskStats.SwapinDelay)/ 1000000d / 1000d : Totals.IoTime;
        public double IoTime_PerCents { get; set; }
        
        // IO Transfer
        public long ReadBytes => Totals.ReadBytes;
        public double ReadBytes_Current { get; set; }
        public long WriteBytes => Totals.WriteBytes;
        public double WriteBytes_Current { get; set; }
        public long ReadBlockBackedBytes => Totals.ReadBlockBackedBytes;
        public double ReadBlockBackedBytes_Current { get; set; }
        public long WriteBlockBackedBytes => Totals.WriteBlockBackedBytes;
        public double WriteBlockBackedBytes_Current { get; set; }

        public long ReadSysCalls => Totals.ReadSysCalls;
        public double ReadSysCalls_Current { get; set; }
        public long WriteSysCalls => Totals.WriteSysCalls;
        public double WriteSysCalls_Current { get; set; }
        
        // Page Faults
        public long MinorPageFaults => Totals.MinorPageFaults;
        public double MinorPageFaults_Current { get; set; }
        public long MajorPageFaults => Totals.MajorPageFaults;
        public double MajorPageFaults_Current { get; set; }
        public long ChildrenMinorPageFaults => Totals.ChildrenMinorPageFaults;
        public double ChildrenMinorPageFaults_Current { get; set; }
        public long ChildrenMajorPageFaults => Totals.ChildrenMajorPageFaults;
        public double ChildrenMajorPageFaults_Current { get; set; }

        public AdvancedProcessStatPoint(ProcessIoStat totals, LinuxTaskStats.LinuxTaskStats taskStats = default(LinuxTaskStats.LinuxTaskStats))
        {
            Totals = totals;
            TaskStats = taskStats;
        }
    }
}