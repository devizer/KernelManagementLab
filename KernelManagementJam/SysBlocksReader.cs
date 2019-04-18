using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Unix;

namespace KernelManagementJam
{
    /*
     * first level at /sys/blocks - device names as folders
     *   - ro file: 0 if not readonly
     *   - removable: 0 if not removable
     *   - stat - io counter
     */
    public class SysBlocksReader
    {
        private static bool IsFakeLinux => Environment.OSVersion.Platform == PlatformID.Win32NT;

        private static string SysBlockPath
        {
            get
            {
                return IsFakeLinux
                    ? "pseudo-root/sys/block"
                    : "/sys/block";
            }
        } 

        public static List<WithDeviceWithVolumes> GetSnapshot()
        {
            var ret = new List<WithDeviceWithVolumes>();
            DirectoryInfo[] sysBlockFolders;
            try
            {
                var di = new DirectoryInfo(SysBlockPath);
                sysBlockFolders = di.GetDirectories();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("The sysfs filesystem is not available via " + SysBlockPath + " path", e);
            }

            foreach (var sysBlockFolder in sysBlockFolders)
            {
                var devFileType = IsFakeLinux
                    ? "BlockDevice"
                    : new UnixSymbolicLinkInfo("/dev/" + sysBlockFolder.Name).FileType.ToString();

                var blockDevice = new WithDeviceWithVolumes
                {
                    DiskKey = sysBlockFolder.Name,
                    DevFileType = devFileType
                };

                blockDevice.StatisticSnapshot = ParseSnapshot(SysBlockPath + "/" + blockDevice.DiskKey);

                if ((blockDevice.StatisticSnapshot.Size ?? 0) > 0)
                {
                    var di = new DirectoryInfo(SysBlockPath + "/" + sysBlockFolder.Name);
                    var volumesFolders = di.GetDirectories(sysBlockFolder.Name + "*");
                    foreach (var volumesFolder in volumesFolders)
                    {
                        var blockVolumeInfo = new WithVolumeInfo
                        {
                            DiskKey = sysBlockFolder.Name,
                            VolumeKey = volumesFolder.Name
                        };

                        blockVolumeInfo.StatisticSnapshot = ParseSnapshot(SysBlockPath + "/" + blockDevice.DiskKey + "/" + blockVolumeInfo.VolumeKey);
                        blockDevice.Volumes.Add(blockVolumeInfo);
                    }

                    ret.Add(blockDevice);
                }
            }

            return ret;
        }

        private static BlockSnapshot ParseSnapshot(string basePath)
        {
            var ret = new BlockSnapshot();

            ret.Size = TryLongValue(basePath + "/size");
            ret.IsReadonly = TryBooleanValue(basePath + "/ro");
            ret.IsRemovable = TryBooleanValue(basePath + "/removable");

            ret.HwSectorSize = TryIntValue(basePath + "/queue/hw_sector_size");
            ret.LogicalBlockSize = TryIntValue(basePath + "/queue/logical_block_size");
            ret.PhysicalBlockSize = TryIntValue(basePath + "/queue/physical_block_size");



            ret.Statistics = ParseStatistic(basePath + "/stat");
            ret.LoopBackingFile = SmallFileReader.ReadFirstLine(basePath + "/loop/backing_file");


            return ret;
        }

        private static BlockStatistics ParseStatistic(string filePath)
        {
            var firstLine = SmallFileReader.ReadFirstLine(filePath);
            if (firstLine == null) return BlockStatistics.Zero;

            var rawColumns = firstLine.Trim().Split(' ').Where(x => x.Length > 0);
            var columns = new List<long>(15);
            foreach (var rawColumn in rawColumns)
            {
                long columnValue;
                if (!long.TryParse(rawColumn, out columnValue))
                    Trace.WriteLine($"Invalid block device value '{firstLine}' from file [{filePath}]");

                columns.Add(columnValue);
            }

            var blockStatistics = new BlockStatistics
            {
                ReadOperations = columns[0],
                ReadOperationsMerged = columns[1],
                ReadSectors = columns[2],
                ReadWaitingMilliseconds = columns[3],
                WriteOperations = columns[4],
                WriteOperationsMerged = columns[5],
                WriteSectors = columns[6],
                WriteWaitingMilliseconds = columns[7],
                IsValid = true
            };

            int pos = 7, columnsCount = columns.Count;
            if (++pos < columnsCount) blockStatistics.InFlightRequests = columns[pos];
            if (++pos < columnsCount) blockStatistics.IoMilliseconds = columns[pos];
            if (++pos < columnsCount) blockStatistics.TimeInQueue = columns[pos];
            if (++pos < columnsCount) blockStatistics.DiscardRequests = columns[pos];
            if (++pos < columnsCount) blockStatistics.DiscardMerges = columns[pos];
            if (++pos < columnsCount) blockStatistics.DiscardSectors = columns[pos];
            if (++pos < columnsCount) blockStatistics.DiscardTicks = columns[pos];

            return blockStatistics;
        }


        private static bool? TryBooleanValue(string fileName)
        {
            var rawRo = SmallFileReader.ReadFirstLine(fileName);
            return !string.IsNullOrEmpty(rawRo) && rawRo != "0";
        }

        private static long? TryLongValue(string fileName)
        {
            var raw = SmallFileReader.ReadFirstLine(fileName);
            long size;
            if (raw != null && long.TryParse(raw, out size))
                return size;

            return null;
        }

        private static int? TryIntValue(string fileName)
        {
            var ret = TryLongValue(fileName);
            return ret.HasValue ? (int) ret.Value : (int?) null;
        }
    }

    public interface IWithStatisticSnapshot
    {
        BlockSnapshot StatisticSnapshot { get; set; }
    }

    public class WithDeviceWithVolumes : IWithStatisticSnapshot
    {
        public WithDeviceWithVolumes()
        {
            Volumes = new List<WithVolumeInfo>();
        }

        public string DiskKey { get; set; }
        public string DevFileType { get; set; }

        public BlockSnapshot StatisticSnapshot { get; set; }
        public IList<WithVolumeInfo> Volumes { get; set; }
    }

    public class WithVolumeInfo : IWithStatisticSnapshot
    {
        public string DiskKey { get; set; }
        public string VolumeKey { get; set; }
        public BlockSnapshot StatisticSnapshot { get; set; }
    }

    public struct BlockSnapshot
    {
        public bool? IsReadonly { get; set; }
        public bool? IsRemovable { get; set; }

        public BlockStatistics Statistics { get; set; }

        // zero for unused loop-devices
        public long? Size { get; set; }
        public int? HwSectorSize { get; set; }
        public int? LogicalBlockSize { get; set; }
        public int? PhysicalBlockSize { get; set; }

        // if not null then it is a loop block device
        public string LoopBackingFile { get; set; }


    }

    /*
  0 read I/Os       requests      number of read I/Os processed
  1 read merges     requests      number of read I/Os merged with in-queue I/O
  2 read sectors    sectors       number of sectors read
  3 read ticks      milliseconds  total wait time for read requests
  4 write I/Os      requests      number of write I/Os processed
  5 write merges    requests      number of write I/Os merged with in-queue I/O
  6 write sectors   sectors       number of sectors written
  7 write ticks     milliseconds  total wait time for write requests
  8 in_flight       requests      number of I/Os currently in flight
  9 io_ticks        milliseconds  total time this block device has been active
 10 time_in_queue   milliseconds  total wait time for all requests
 11 discard I/Os    requests      number of discard I/Os processed
 12 discard merges  requests      number of discard I/Os merged with in-queue I/O
 13 discard sectors sectors       number of sectors discarded
 14 discard ticks   milliseconds  total wait time for discard requests
     */
    // The line from /sys/blocks/<dev>/stat
    public struct BlockStatistics
    {
        //0
        public long ReadOperations { get; set; }

        //1
        public long ReadOperationsMerged { get; set; }

        //2
        public long ReadSectors { get; set; }

        //3
        public long ReadWaitingMilliseconds { get; set; }

        //4
        public long WriteOperations { get; set; }

        //5
        public long WriteOperationsMerged { get; set; }

        //6
        public long WriteSectors { get; set; }

        //7
        public long WriteWaitingMilliseconds { get; set; }

        // 8
        public long InFlightRequests { get; set; }

        // 9
        public long IoMilliseconds { get; set; }

        // 10. milliseconds,  total wait time for all requests
        public long? TimeInQueue { get; set; }

        // 11. number of discard I/Os processed
        public long? DiscardRequests { get; set; }

        // 12. requests, number of discard I/Os merged with in-queue I/O
        public long? DiscardMerges { get; set; }

        // 13. number of sectors discarded
        public long? DiscardSectors { get; set; }

        // 14. milliseconds,  total wait time for discard requests
        public long? DiscardTicks { get; set; }

        public bool IsValid { get; set; }

        public static BlockStatistics Zero => new BlockStatistics();

        public bool IsDead => ReadOperations == 0 &&
                              ReadOperationsMerged == 0 &&
                              ReadSectors == 0 &&
                              ReadWaitingMilliseconds == 0 &&
                              WriteOperations == 0 &&
                              WriteOperationsMerged == 0 &&
                              WriteSectors == 0 &&
                              WriteWaitingMilliseconds == 0;

        public static BlockStatistics GetDelta(BlockStatistics next, BlockStatistics prev, double duration)
        {
            return new BlockStatistics()
            {
                ReadOperations = GetDelta(next.ReadOperations, prev.ReadOperations , duration),
                ReadOperationsMerged = GetDelta(next.ReadOperationsMerged, prev.ReadOperationsMerged , duration),
                ReadSectors = GetDelta(next.ReadSectors, prev.ReadSectors , duration),
                ReadWaitingMilliseconds = GetDelta(next.ReadWaitingMilliseconds, prev.ReadWaitingMilliseconds , duration),
                WriteOperations = GetDelta(next.WriteOperations, prev.WriteOperations , duration),
                WriteOperationsMerged = GetDelta(next.WriteOperationsMerged, prev.WriteOperationsMerged , duration),
                WriteSectors = GetDelta(next.WriteSectors, prev.WriteSectors , duration),
                WriteWaitingMilliseconds = GetDelta(next.WriteWaitingMilliseconds, prev.WriteWaitingMilliseconds , duration),
                InFlightRequests = GetDelta(next.InFlightRequests, prev.InFlightRequests , duration),
                IoMilliseconds = GetDelta(next.IoMilliseconds, prev.IoMilliseconds , duration),
                TimeInQueue = GetDelta(next.TimeInQueue, prev.TimeInQueue , duration),
                DiscardRequests = GetDelta(next.DiscardRequests, prev.DiscardRequests , duration),
                DiscardMerges = GetDelta(next.DiscardMerges, prev.DiscardMerges, duration),
                DiscardSectors = GetDelta(next.DiscardSectors, prev.DiscardSectors, duration),
                DiscardTicks = GetDelta(next.DiscardTicks, prev.DiscardTicks , duration),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static long GetDelta(long next, long prev, double duration)
        {
            return (long) ((next - prev) / duration);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static long? GetDelta(long? next, long? prev, double duration)
        {
            if (!next.HasValue) return null;
            long p = prev ?? 0;
            return (long) ((next.Value - p) / duration);
        }
    }
}