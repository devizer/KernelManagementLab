using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Universe.LinuxTaskStats;

namespace Universe.Dashboard.Agent
{
    public class ProcessListTimer_Smarty
    {
        public static readonly ProcessListTimer_Smarty Instance = new ProcessListTimer_Smarty();

        private ProcessListTimer_Smarty()
        {
        }

        class State // immutable
        {
            public double At; // milliseconds

            public List<ProcessIoStat> Snapshot;
            public Dictionary<ProcessIoStatKey, ProcessIoStat> ByKeys;

            public List<LinuxTaskStats.LinuxTaskStats> TaskStatsSnapshot;
            public Dictionary<ProcessIoStatKey, LinuxTaskStats.LinuxTaskStats> TaskStatsByKeys;
            
        }
        
        // Depends on WholeTime only
        State BuildCurrentState(ProcessIoStat[] processes, List<LinuxTaskStats.LinuxTaskStats> taskStats)
        {
            // 
            State next = new State()
            {
                At = WholeTime.ElapsedTicks * 1000d / Stopwatch.Frequency,
                Snapshot = processes.Where(x => !x.IsZombie).ToList(),
                TaskStatsSnapshot = taskStats,
            };
            
            next.ByKeys = new Dictionary<ProcessIoStatKey, ProcessIoStat>(next.Snapshot.Count, ProcessIoStatKey.EqualityComparer);
            foreach (var process in next.Snapshot)
                next.ByKeys[new ProcessIoStatKey() {Pid = process.Pid, StartAtRaw = process.StartAtRaw}] = process;
            
            next.TaskStatsByKeys = new Dictionary<ProcessIoStatKey, LinuxTaskStats.LinuxTaskStats>(next.Snapshot.Count, ProcessIoStatKey.EqualityComparer);
            foreach (var taskStat in taskStats)
                next.TaskStatsByKeys[new ProcessIoStatKey() {Pid = taskStat.Pid, StartAtRaw = taskStat.BeginTime32}] = taskStat;

            return next;
        }
        
        
        private State Next;
        Stopwatch WholeTime = Stopwatch.StartNew();
        
        // если прошло недолго с ActualListAt то возвращаем ActualList
        private List<AdvancedProcessStatPoint> ActualList = null;
        private long ActualListBuildTime = 0;
        object ActualListSync = new object();
        private long ActualListRequestTime = 0;
        
        AutoResetEvent NotifyRequest = new AutoResetEvent(false);

        public List<AdvancedProcessStatPoint> GetProcesses()
        {
            NotifyRequest.Set();
            ActualListRequestTime = WholeTime.ElapsedMilliseconds;
            int step = 100, count = 90, n = 0;
            while (n++ < count)
            {
                long now = WholeTime.ElapsedMilliseconds;
                // lock (ActualListSync)
                {
                    if (now - ActualListBuildTime < 2000)
                    {
                        double uptime = UptimeParser.ParseUptime().Value;
                        foreach (var proc in ActualList)
                        {
                            proc.Uptime = uptime - proc.Totals.StartAt;
                        }

                        return ActualList;
                    }
                }
                Thread.Sleep(step);
            }
            
            throw new ApplicationException($"The System is too busy. Unable to obtain ProcessList for {(count*step):n0} milliseconds");
        }

        public void Process()
        {
            Thread t = new Thread(_ => ProcessThread()) { Name = "ProcessList", IsBackground = true};
            t.Start();
        }

        private void ProcessThread()
        {
            while (true)
            {
                bool forceUpdate = NotifyRequest.WaitOne(600); // 50 works wrong - reports for self too much CPU USage
                long now = WholeTime.ElapsedMilliseconds;
                bool continueUpdate = now - ActualListRequestTime < 5000;
                bool needUpdate = now - ActualListBuildTime > 999;
                var msg = $"Now: {now:n0}  RequestTime: {ActualListRequestTime} BuildTime: {ActualListBuildTime}";
                if (continueUpdate && needUpdate || Next == null)
                {

                    AdvancedMiniProfilerStep GetProfilerSubStep(string subStepName)
                    {
                        return AdvancedMiniProfiler.Step(SharedDefinitions.RootKernelMetricsObserverKey, "ProcessList", subStepName);
                    }

                    ProcessIoStat[] processes;
                    using (GetProfilerSubStep("1. Get ProcessIoStat Snapshot"))
                    {
                        // may fail?
                        processes = ProcessIoStat.GetProcesses()
                            .Where(x => !x.IsZombie)
                            .ToArray(); 
                    }

                    List<LinuxTaskStats.LinuxTaskStats> taskStats = new List<LinuxTaskStats.LinuxTaskStats>(processes.Length);
                    using (GetProfilerSubStep("2. Get LinuxTaskStats Snapshot"))
                    {
                        for (int i = 0; i < processes.Length; i++)
                        {
                            if (LinuxTaskStatsReader.IsGetTaskStatByProcessSupported)
                            {
                                // Permissions are required: in debugger it returns null
                                LinuxTaskStats.LinuxTaskStats? taskStat = LinuxTaskStatsReader.GetByProcess(processes[i].Pid);
                                if (taskStat.HasValue) taskStats.Add(taskStat.Value);
                            }
                        }

                        DebugDumper.Dump(taskStats, "LinuxTaskStats.json");
                    }

                    State next;
                    using (GetProfilerSubStep("3. Build Immutable State"))
                    {
                       next = BuildCurrentState(processes, taskStats);
                    }

                    if (Next != null)
                    {
                        // Console.WriteLine($"next.At - Next.At = {next.At - Next.At} milliseconds");
                    }
                    bool isRecently = Next != null && next.At - Next.At < 1000d;
                    bool atLeastHalfSeconds = Next != null && next.At - Next.At >= 500d;
                    if (Next != null /*&& isRecently */ /*&& atLeastHalfSeconds*/)
                    {
                        using (GetProfilerSubStep("4. Compute Delta (build ActualList)"))
                        {
                            List<AdvancedProcessStatPoint> newList = new List<AdvancedProcessStatPoint>();
                            foreach (var process in next.Snapshot)
                            {
                                ProcessIoStatKey key = new ProcessIoStatKey() {Pid = process.Pid, StartAtRaw = process.StartAtRaw};
                                if (Next.ByKeys.TryGetValue(key, out var prevProcess))
                                {
                                    double dur = (next.At - Next.At) / 1000;
                                    // nice. TODO: calc delta and add row to ActualList
                                    AdvancedProcessStatPoint actualProcess = new AdvancedProcessStatPoint(process);
                                    // IO Transfer
                                    actualProcess.ReadBytes_Current = (process.ReadBytes - prevProcess.ReadBytes) / dur;
                                    actualProcess.WriteBytes_Current = (process.WriteBytes - prevProcess.WriteBytes) / dur;
                                    actualProcess.ReadSysCalls_Current = (process.ReadSysCalls - prevProcess.ReadSysCalls) / dur;
                                    actualProcess.WriteSysCalls_Current = (process.WriteSysCalls - prevProcess.WriteSysCalls) / dur;
                                    actualProcess.ReadBlockBackedBytes_Current = (process.ReadBlockBackedBytes - prevProcess.ReadBlockBackedBytes) / dur;
                                    actualProcess.WriteBlockBackedBytes_Current = (process.WriteBlockBackedBytes - prevProcess.WriteBlockBackedBytes) / dur;
                                    // IO Time
                                    actualProcess.IoTime_PerCents = (process.IoTime - prevProcess.IoTime) / dur;
                                    // Page Faults
                                    actualProcess.MinorPageFaults_Current = (process.MinorPageFaults - prevProcess.MinorPageFaults) / dur;
                                    actualProcess.MajorPageFaults_Current = (process.MajorPageFaults - prevProcess.MajorPageFaults) / dur;
                                    actualProcess.ChildrenMinorPageFaults_Current = (process.ChildrenMinorPageFaults - prevProcess.ChildrenMinorPageFaults) / dur;
                                    actualProcess.ChildrenMajorPageFaults_Current = (process.ChildrenMajorPageFaults - prevProcess.ChildrenMajorPageFaults) / dur;
                                    // CPU Usage
                                    actualProcess.UserCpuUsage_PerCents = (process.UserCpuUsage - prevProcess.UserCpuUsage) / dur;
                                    actualProcess.KernelCpuUsage_PerCents = (process.KernelCpuUsage - prevProcess.KernelCpuUsage) / dur;
                                    actualProcess.TotalCpuUsage_PerCents = (process.UserCpuUsage + process.KernelCpuUsage - (prevProcess.UserCpuUsage + prevProcess.KernelCpuUsage)) / dur;
                                    actualProcess.ChildrenUserCpuUsage_PerCents = (process.ChildrenUserCpuUsage - prevProcess.ChildrenUserCpuUsage) / dur;
                                    actualProcess.ChildrenKernelCpuUsage_PerCents = (process.ChildrenKernelCpuUsage - prevProcess.ChildrenKernelCpuUsage) / dur;
                                    actualProcess.ChildrenTotalCpuUsage_PerCents = (process.ChildrenUserCpuUsage + process.ChildrenKernelCpuUsage - (prevProcess.ChildrenUserCpuUsage + prevProcess.ChildrenKernelCpuUsage)) / dur;
                                    // Whatever more?
                                    // actualProcess. = (process. - prevProcess.) / dur;
                                    newList.Add(actualProcess);
                                }
                                else
                                {
                                    // really?
                                    // AdvancedProcessStatPoint actualProcess = new AdvancedProcessStatPoint(process);
                                    // newList.Add(actualProcess);
                                }
                            }

                            var buildAt = WholeTime.ElapsedMilliseconds;
                            lock (ActualListSync)
                            {
                                this.ActualList = newList;
                                this.ActualListBuildTime = buildAt;
                            }
                        }
                    }

                    Next = next;
                }
            }
        }

    }

    class ProcessIoStatKey
    {
        public int Pid;
        public long StartAtRaw;

        private sealed class PidStartAtRawEqualityComparer : IEqualityComparer<ProcessIoStatKey>
        {
            public bool Equals(ProcessIoStatKey x, ProcessIoStatKey y)
            {
                if (ReferenceEquals(x, y)) return true;
                return x.Pid == y.Pid && x.StartAtRaw == y.StartAtRaw;
            }

            public int GetHashCode(ProcessIoStatKey obj)
            {
                return ((int)(obj.StartAtRaw & 0xFFFFFFFF) * 397) ^ obj.Pid;
                return HashCode.Combine(obj.Pid, obj.StartAtRaw);
            }
        }

        public static readonly IEqualityComparer<ProcessIoStatKey> EqualityComparer = new PidStartAtRawEqualityComparer();
    }
}
/*
 
*/