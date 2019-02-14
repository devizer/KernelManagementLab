using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using KernelManagementJam;

namespace MountLab
{
    class SysBlockMonitorV1
    {
        public static void RunMonV1()
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<BlockDeviceWithVolumes> prev = SysBlocksReader.GetSnapshot();
            var prevTicks = sw.ElapsedTicks;
            var prevGrouped = AsDictionary(prev, x => x.DiskKey);

            Console.Clear();

            while (true)
            {
                Thread.Sleep(1);

                List<BlockDeviceWithVolumes> next = SysBlocksReader.GetSnapshot();
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
                foreach (BlockDeviceWithVolumes block in next)
                {
                    pos++;
                    var isFound = prevGrouped.TryGetValue(block.DiskKey, out var prevBlock);
                    if (!isFound) continue;
                    List<object> cells = new List<object> {pos, block.DiskKey, null};
                    Add(cells, x => x.ReadOperations, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.ReadOperationsMerged, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.ReadSectors, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.ReadWaitingMilliseconds, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.WriteOperations, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.WriteOperationsMerged, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.WriteSectors, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.WriteWaitingMilliseconds, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.InFlightRequests, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.IoMilliseconds, block.Device, prevBlock.Device, duration);
                    Add(cells, x => x.TimeInQueue, block.Device, prevBlock.Device, duration);
                    report.AddRow(cells.ToArray());
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

        static List<object> Add(List<object> cells, Func<BlockStatistics, long> field, BlockSnapshot next, BlockSnapshot prev, double duration)
        {
            long nextValue = field(next.Statistics);
            long prevValue = field(prev.Statistics);
            cells.Add(nextValue);
            cells.Add(nextValue - prevValue);
            return cells;
        }

        static List<object> Add(List<object> cells, Func<BlockStatistics, long?> field, BlockSnapshot next, BlockSnapshot prev, double duration)
        {
            long? nextValue = field(next.Statistics);
            long? prevValue = field(prev.Statistics);
            cells.Add(nextValue.HasValue ? nextValue.Value : (object) null);
            cells.Add(nextValue.HasValue ? (nextValue.Value - prevValue.Value) : (object) null);
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