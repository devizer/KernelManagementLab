using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Universe.Dashboard.Agent
{
    public class BlockDiskTimer
    {
        private static readonly UTF8Encoding Utf8Encoding = new UTF8Encoding(false);

        public static void Process()
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            List<WithDeviceWithVolumes> prev = SysBlocksReader.GetSnapshot();
            var prevTicks = sw.ElapsedTicks;
            var prevPlain = AsPlainVolsAndDisks(prev);
            
            PreciseTimer.AddListener("SysBlock::Timer", () =>
            {
                List<WithDeviceWithVolumes> next = SysBlocksReader.GetSnapshot();
                var nextTicks = sw.ElapsedTicks;
                var nextPlain = AsPlainVolsAndDisks(next);
                var at = DateTime.UtcNow;
                double duration = (nextTicks - prevTicks) * 1d / Stopwatch.Frequency;
                
                DebugDumper.Dump(next, "SysBlock.Timer.Tick.Next.json");
                // Console.WriteLine("SysBlock::Timer --> Temp Tick1");

                List<DiskVolStatModel> totals = new List<DiskVolStatModel>();
                List<DiskVolStatModel> nextDelta = new List<DiskVolStatModel>();
                foreach (var pair in nextPlain)
                {
                    var nextStat = pair.Value;
                    var diskOrVolumeKey = pair.Key;
                    if (!prevPlain.TryGetValue(diskOrVolumeKey, out var prevStat))
                        prevStat = BlockStatistics.Zero;

                    var delta = BlockStatistics.GetDelta(nextStat, prevStat, duration);
                    nextDelta.Add(new DiskVolStatModel()
                    {
                        Kind = "Not Implemented",
                        DiskVolKey = diskOrVolumeKey,
                        Stat = delta,
                    });
                    
                    totals.Add(new DiskVolStatModel()
                    {
                        Kind = "Not Implemented",
                        DiskVolKey = diskOrVolumeKey,
                        Stat = nextStat,
                    });
                }

                nextDelta = nextDelta.OrderBy(x => x.DiskVolKey).ToList();
                
                BlockDiskDataSourcePoint point = new BlockDiskDataSourcePoint()
                {
                    At = at,
                    BlockDiskStat = nextDelta,
                };

                var logBy1Seconds = BlockDiskDataSource.Instance.By_1_Seconds;
                while (logBy1Seconds.Count >= 60 + 1)
                    logBy1Seconds.RemoveAt(0);
                
                logBy1Seconds.Add(point);
                BlockDiskDataSource.Instance.Totals = totals;


                DebugDumper.Dump(logBy1Seconds, "BlockDiskDataSource.1s.json");
                DebugDumper.Dump(logBy1Seconds, "BlockDiskDataSource.1s.min.json", minify: true);

                var viewModel1s = BlockDiskDataSourceView.AsViewModel(logBy1Seconds);
                DebugDumper.Dump(viewModel1s, "Block.View.1s.json");
                DebugDumper.Dump(viewModel1s, "Block.View.1s.min.json", minify: true);
                
                prev = next;
                prevTicks = nextTicks;
                prevPlain = nextPlain;
            });
        }
        
        // Key:
        //      .VolumeKey for volumes
        //      .DiskKey for disks
        static Dictionary<string, BlockStatistics> AsPlainVolsAndDisks(List<WithDeviceWithVolumes> snapshot)
        {
            Dictionary<string, BlockStatistics> ret = new Dictionary<string, BlockStatistics>();
            foreach (WithDeviceWithVolumes block in snapshot)
            {
                var diskStat = block.StatisticSnapshot.Statistics;
                if (block.Volumes.Count == 1)
                {
                    var singleVolume = block.Volumes.First();
                    var volStat = singleVolume.StatisticSnapshot.Statistics;
                    if (!volStat.IsDead)
                    {
                        if (IsActive(singleVolume.StatisticSnapshot))
                            ret[singleVolume.VolumeKey] = volStat;
                        
                        continue;
                    }
                }
                
                if (!diskStat.IsDead && IsActive(block.StatisticSnapshot))
                    ret[block.DiskKey] = diskStat;
                
                foreach (WithVolumeInfo volume in block.Volumes)
                {
                    if (!volume.StatisticSnapshot.Statistics.IsDead && IsActive(volume.StatisticSnapshot))
                        ret[volume.VolumeKey] = volume.StatisticSnapshot.Statistics;
                }
            }

            return ret;
        }

        static bool IsActive(BlockSnapshot snapshot)
        {
            int? blockSize = snapshot.HwSectorSize ?? snapshot.LogicalBlockSize ?? snapshot.PhysicalBlockSize;
            var traffic = (snapshot.Statistics.ReadSectors + snapshot.Statistics.WriteSectors) * (blockSize ?? 512);
            var threshold = BlockDiskEnv.BlockDeviceVisibilityThreshold * 1024;
            return (threshold == 0 && traffic > 0) || (traffic >= threshold);
        }

        class BlockDiskEnv
        {
            private const string VisibilityThresholdEnvName = "BLOCK_DEVICE_VISIBILITY_THRESHOLD";
            private const long DefaultBlockDeviceVisibilityThreshold = 2048;

            public static long BlockDeviceVisibilityThreshold => _BlockDeviceVisibilityThreshold.Value;  

            private static Lazy<long> _BlockDeviceVisibilityThreshold = new Lazy<long>(() =>
            {
                var raw = Environment.GetEnvironmentVariable(VisibilityThresholdEnvName);
                long.TryParse(raw, out var ret);
                ret = Math.Max(ret, 0);
                Console.WriteLine($"BLOCK_DEVICE_VISIBILITY_THRESHOLD: {ret} Kb");
                return ret;
            });
            
        }
    }
}   