using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using KernelManagementJam;

namespace MountLab
{
    class SysBlockMonitorV1
    {
        public static void RunMonV1()
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<WithDeviceWithVolumes> prev = SysBlocksReader.GetSnapshot();
            var prevTicks = sw.ElapsedTicks;
            var prevGrouped = AsDictionary(prev, x => x.DiskKey);

            Console.Clear();

            while (true)
            {
                Thread.Sleep(1);

                List<WithDeviceWithVolumes> next = SysBlocksReader.GetSnapshot();
                var nextTicks = sw.ElapsedTicks;
                var duration = (nextTicks - prevTicks) / (double) Stopwatch.Frequency;

                ConsoleTable report = new ConsoleTable("", "Dev", "Mount",
                    "-RdOps",
                    "- ± ",
                    "-RdOps-M",
                    "- ± ",
                    "-RdSct",
                    "- ± ",
                    "-RdWait",
                    "- ± ",
                    "-WrOps",
                    "- ± ",
                    "-WrOps-M",
                    "- ± ",
                    "-WrSct",
                    "- ± ",
                    "-WrWait",
                    "- ± ",
                    "-QueReqs",
                    "- ± ",
                    "-mSec",
                    "- ± ",
                    "-TimeInQueue",
                    "- ± "
                );

                int pos = 0;
                foreach (WithDeviceWithVolumes block in next)
                {
                    pos++;
                    var isFound = prevGrouped.TryGetValue(block.DiskKey, out var prevBlock);
                    if (!isFound) continue;
                    List<object> cellsOfDisk = new List<object> {pos, block.DiskKey, null};
                    AddStat(cellsOfDisk, block, prevBlock, duration);
                    report.AddRow(cellsOfDisk.ToArray());

                    foreach (var vol in block.Volumes)
                    {
                        var prevVol = prevBlock.Volumes.Where(x => vol.VolumeKey.Equals(x.VolumeKey)).FirstOrDefault();
                        if (prevVol == null) continue;

                        List<object> cellsOfVol = new List<object> { null, " |-- " + vol.VolumeKey, null };
                        AddStat(cellsOfVol, vol, prevVol, duration);
                        report.AddRow(cellsOfDisk.ToArray());
                    }
                }

                var reportAsString = report.ToString();
                Console.Clear();
                Console.SetCursorPosition(0,0);
                Console.WriteLine(reportAsString);

                prev = next;
                prevTicks = nextTicks;
                prevGrouped = AsDictionary(prev, x => x.DiskKey);


                Thread.Sleep(1000);
            }
        }

        static void AddStat(List<object> cells, IWithStatisticSnapshot next, IWithStatisticSnapshot prev, double duration)
        {
            Add(cells, x => x.ReadOperations, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.ReadOperationsMerged, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.ReadSectors, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.ReadWaitingMilliseconds, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.WriteOperations, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.WriteOperationsMerged, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.WriteSectors, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.WriteWaitingMilliseconds, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.InFlightRequests, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.IoMilliseconds, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
            Add(cells, x => x.TimeInQueue, next.StatisticSnapshot, prev.StatisticSnapshot, duration);
        }

        static List<object> Add(List<object> cells, Func<BlockStatistics, long> field, BlockSnapshot next, BlockSnapshot prev, double duration)
        {
            long nextValue = field(next.Statistics);
            long prevValue = field(prev.Statistics);
            cells.Add(nextValue);
            cells.Add(Convert.ToInt64((nextValue - prevValue) / duration));
            return cells;
        }

        static List<object> Add(List<object> cells, Func<BlockStatistics, long?> field, BlockSnapshot next, BlockSnapshot prev, double duration)
        {
            long? nextValue = field(next.Statistics);
            long? prevValue = field(prev.Statistics);
            cells.Add(nextValue.HasValue ? nextValue.Value : (object) null);
            cells.Add(nextValue.HasValue ? Convert.ToInt64((nextValue.Value - prevValue.Value)/duration) : (object) null);
            return cells;
        }

        static IDictionary<string, T> AsDictionary<T>(IEnumerable<T> list, Func<T,string> key)
        {
            Dictionary<string,T> ret = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var item in list)
            {
                ret[key(item)] = item;
            }

            return ret;
        }
    }
}