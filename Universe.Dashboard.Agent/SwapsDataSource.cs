using System;
using System.Collections.Generic;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Microsoft.EntityFrameworkCore.Design;

namespace Universe.Dashboard.Agent
{
    public class SwapsDataSource
    {
        static List<SwapInfo> _Swaps;
        static readonly object Sync = new object();
        public static ManualResetEvent IsFirstIterationReady = new ManualResetEvent(false);
        
        static SwapsDataSource()
        {
            LaunchListener();
        }
        
        public static List<SwapInfo> Swaps
        {
            get
            {
                IsFirstIterationReady.WaitOne();
                lock (Sync) return _Swaps;
            }
            set
            {
                lock (Sync) _Swaps = value;
            }
        }

        static void LaunchListener()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                while (!PreciseTimer.Shutdown.WaitOne(0))
                {
                    using (AdvancedMiniProfiler.Step(SharedDefinitions.RootKernelMetricsObserverKey, "SwapsDataSource.Iteration()"))
                    {
                        try
                        {
                            Iteration();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning! {nameof(SwapsDataSource)}.{nameof(Iteration)} failed.{Environment.NewLine}{ex}");
                        }
                    }
                    
                    IsFirstIterationReady.Set();
                    PreciseTimer.Shutdown.WaitOne(1000);
                }
            });
        }
        
        static void Iteration()
        {
            bool isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
            if (isWin)
            {
                Swaps = new List<SwapInfo>();
                return;
            }

            var swaps = SwapsParser.Parse();
            Swaps = swaps;
            
            DebugDumper.Dump(swaps, "ProcSwaps.json");
            DebugDumper.Dump(swaps, "ProcSwaps.min.json", true);
        }
    }
}
