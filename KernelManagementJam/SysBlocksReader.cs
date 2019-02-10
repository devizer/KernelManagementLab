using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        static readonly string SysBlockPath = "/sys/block";
        public static List<BlockDeviceWithVolumes> GetSnapshot()
        {
            List<BlockDeviceWithVolumes> ret = new List<BlockDeviceWithVolumes>();
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
                Mono.Unix.UnixSymbolicLinkInfo i = new Mono.Unix.UnixSymbolicLinkInfo("/dev/" + sysBlockFolder.Name);
                BlockDeviceWithVolumes blockDevice = new BlockDeviceWithVolumes()
                {
                    ShortKey = sysBlockFolder.Name,
                    DevFileType = i.FileType.ToString(),
                };

                blockDevice.Device = ParseSnapshot(SysBlockPath + "/" + blockDevice.ShortKey);

                if ((blockDevice.Device.Size ?? 0) > 0)
                    ret.Add(blockDevice);
            }

            return ret;
        }

        static BlockSnapshot ParseSnapshot(string basePath)
        {
            BlockSnapshot ret = new BlockSnapshot();

            ret.Size = TryLongValue(basePath + "/size");
            ret.IsReadonly = TryBooleanValue(basePath + "/ro");
            ret.IsRemovable = TryBooleanValue(basePath + "/removable");

            ret.HwSectorSize = TryIntValue(basePath + "/queue/hw_sector_size");
            ret.LogicalBlockSize = TryIntValue(basePath + "/queue/logical_block_size");
            ret.PhysicalBlockSize = TryIntValue(basePath + "/queue/physical_block_size");

            ret.Statistics = ParseStatistic(basePath + "/stat");

            return ret;
        }

        private static BlockStatistics ParseStatistic(string filePath)
        {
            var firstLine = SmallFileReader.ReadFirstLine(filePath);
            if (firstLine == null) return BlockStatistics.Empty;

            var rawColumns = firstLine.Trim().Split(' ').Where(x => x.Length > 0);
            List<long> columns = new List<long>(8);
            foreach (var rawColumn in rawColumns)
            {
                if (!long.TryParse(rawColumn, out var column))
                    Trace.WriteLine($"Invalid block device value '{firstLine}' from file [{filePath}]");

                columns.Add(column);
            }

            return new BlockStatistics
            {
                ReadOperations = columns[0],
                ReadOperationsMerged = columns[1],
                ReadSectors = columns[2],
                ReadWaitingMilliseconds = columns[3],
                WriteOperations = columns[4],
                WriteOperationsMerged = columns[5],
                WriteSectors = columns[6],
                WriteWaitingMilliseconds = columns[7],
                IsValid = true,
            };
        }


        static bool? TryBooleanValue(string fileName)
        {
                var rawRo = SmallFileReader.ReadFirstLine(fileName);
                return !string.IsNullOrEmpty(rawRo) && rawRo != "0";
        }

        static long? TryLongValue(string fileName)
        {
            var raw = SmallFileReader.ReadFirstLine(fileName);
            if (raw != null && long.TryParse(raw, out var size))
                return size;

            return null;
        }

        static int? TryIntValue(string fileName)
        {
            var ret = TryLongValue(fileName);
            return ret.HasValue ? (int) ret.Value : (int?) null;
        }
    }

    public class BlockDeviceWithVolumes
    {
        public string ShortKey { get; set; }
        public string DevFileType { get; set; }

        public BlockSnapshot Device { get; set; }
        public IList<BlockVolumeInfo> Volumes { get; set; }
    }

    public class BlockVolumeInfo
    {
        public string DevKey { get; set; }
        public BlockSnapshot Volume { get; set; }
        public string VolumeKey { get; set; }
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

        public bool IsValid { get; set; }

        public static BlockStatistics Empty => new BlockStatistics();
    }
}
