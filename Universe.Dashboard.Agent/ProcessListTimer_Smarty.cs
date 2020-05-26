using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;

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
            public List<ProcessIoStat> Snapshot;
            public double At; // milliseconds
            public Dictionary<ProcessIoStatKey, ProcessIoStat> ByKeys;
        }
        
        // Depends on WholeTime only
        State BuildCurrentState(ProcessIoStat[] processes)
        {
            // 
            State next = new State()
            {
                At = WholeTime.ElapsedTicks * 1000d / Stopwatch.Frequency,
                Snapshot = processes.Where(x => !x.IsZombie).ToList()
            };
            next.ByKeys = new Dictionary<ProcessIoStatKey, ProcessIoStat>(next.Snapshot.Count, ProcessIoStatKey.EqualityComparer);
            foreach (var process in next.Snapshot)
                next.ByKeys[new ProcessIoStatKey() {Pid = process.Pid, StartAtRaw = process.StartAtRaw}] = process;

            return next;
        }
        
        
        private State Prev;
        private State Next;
        Stopwatch WholeTime = Stopwatch.StartNew();
        
        // если прошло недолго с ActualListAt то возвращаем ActualList
        private List<AdvancedProcessStatPoint> ActualList = null;
        private long ActualListBuildTime = 0;
        object ActualListSync = new object();
        private long ActualListRequestTime = 0;
        
        ManualResetEvent NotifyActualList = new ManualResetEvent(false);
        
        AutoResetEvent NotifyRequest = new AutoResetEvent(false);
        

        public List<AdvancedProcessStatPoint> GetProcesses()
        {
            NotifyRequest.Set();
            int step = 100, count = 90, n = 0;
            while (n++ < count)
            {
                lock (ActualListSync)
                {
                    long now = WholeTime.ElapsedMilliseconds;
                    if (now - ActualListBuildTime < 1001) return ActualList;
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
                bool forceUpdate = NotifyRequest.WaitOne(100);
                long now = WholeTime.ElapsedMilliseconds;
                bool continueUpdate = now - ActualListRequestTime < 5000;
                bool needUpdate = now - ActualListBuildTime < 5000;
                if (forceUpdate || continueUpdate || needUpdate)
                {

                    AdvancedMiniProfilerStep GetProfilerSubStep(string subStepName)
                    {
                        return AdvancedMiniProfiler.Step(SharedDefinitions.RootKernelMetricsObserverKey, "ProcessList", subStepName);
                    }

                    ProcessIoStat[] processes;
                    using (GetProfilerSubStep("1. GetSnapshot"))
                    {
                        processes = ProcessIoStat.GetProcesses(); // may fail?
                    }

                    State next;
                    using (GetProfilerSubStep("2. Build Immutable State"))
                    {
                       next = BuildCurrentState(processes);
                    }
                    
                    bool isRecently = Next != null && next.At - Next.At < 1000d;
                    bool atLeastHalfSeconds = Next != null && next.At - Next.At >= 500d;
                    if (Next != null && isRecently && atLeastHalfSeconds)
                    {
                        using (GetProfilerSubStep("3. Compute Delta (build ActualList"))
                        {
                            List<AdvancedProcessStatPoint> newList = new List<AdvancedProcessStatPoint>();
                            foreach (var process in next.Snapshot)
                            {
                                ProcessIoStatKey key = new ProcessIoStatKey() {Pid = process.Pid, StartAtRaw = process.StartAtRaw};
                                if (Next.ByKeys.TryGetValue(key, out var prevProcess))
                                {
                                    // nice. TODO: calc delta and add row to ActualList
                                    AdvancedProcessStatPoint actualProcess = new AdvancedProcessStatPoint(process);
                                    actualProcess.ReadBytes_Current = process.ReadBytes - prevProcess.ReadBytes;
                                    actualProcess.WriteBytes_Current = process.WriteBytes - prevProcess.WriteBytes;
                                    // ...
                                    newList.Add(actualProcess);
                                }
                                else
                                {
                                    // really?
                                    // AdvancedProcessStatPoint actualProcess = new AdvancedProcessStatPoint(process);
                                    // newList.Add(actualProcess);
                                }
                            }
                        }
                    }

                    Prev = Next;
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