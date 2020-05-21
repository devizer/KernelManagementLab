using System;
using KernelManagementJam;

namespace Universe.Dashboard.Agent
{
    public class ProcessListDataSource
    {
    }

    public class AdvancedProcess
    {
        private ProcessIoStat Totals { get; set; }

        // Process
        public int Pid => Totals.Pid;
        public int ParentPid => Totals.ParentPid;
        public string User => string.IsNullOrEmpty(Totals.UserName) ? $"{Totals.Uid}" : $"{Totals.Uid} '{Totals.UserName}'";
        public int MixedPriority => (int) Totals.MixedPriority; // Nice: 0...39 is -20..19. Realtime: -2 ==> 1, -3 ==> 2, ... -100 ==> 99
        public long Uptime => throw new NotImplementedException(); // in seconds

        // Memory
        public long RSS => Totals.RssMem;
        public long Shared => Totals.SharedMem;
        public long Swapped => Totals.SwappedMem;

        // CPU Usage
        public double UserCpuUsage => Totals.UserCpuUsage / 100; // seconds
        public double UserCpuUsage_PerCents { get; set; }
        public double KernelCpuUsage => Totals.KernelCpuUsage / 100; // seconds
        public double KernelCpuUsage_PerCents { get; set; }
        public double TotalCpuUsage => (Totals.UserCpuUsage + Totals.KernelCpuUsage) / 100; // seconds
        public double TotalCpuUsage_PerCents { get; set; }

        // Children CPU Usage
        public double ChildrenUserCpuUsage => Totals.ChildrenUserCpuUsage / 100; // seconds
        public double ChildrenUserCpuUsage_PerCents { get; set; }
        public double ChildrenKernelCpuUsage => Totals.ChildrenKernelCpuUsage / 100; // seconds
        public double ChildrenKernelCpuUsage_PerCents { get; set; }
        public double ChildrenTotalCpuUsage => (Totals.ChildrenUserCpuUsage + Totals.ChildrenKernelCpuUsage) / 100; // seconds
        public double ChildrenTotalCpuUsage_PerCents { get; set; }
        
        // IO Time
        public long IoTime => Totals.IoTime / 100;
        public double IoTime_PerCents { get; set; }
        



    }

}