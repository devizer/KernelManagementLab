using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using KernelManagementJam;

namespace Universe.Dashboard.Agent
{
    public class BlockDiskDataSourceView
    {
        public static List<string> GetDiskOrVolNames()
        {
            List<DiskVolStatModel> totals = BlockDiskDataSource.Instance.Totals;
            if (totals == null) return null;

            // UI friendly sort order is BlockDevicesUI.GetOrderedBlockNames()
            return totals
                .Select(x => x.DiskVolKey)
                .OrderBy(x => x)
                .ToList();
        }
        
        public static object AsViewModel(List<BlockDiskDataSourcePoint> dataSource)
        {
            List<string> blockNames = dataSource
                .SelectMany(x => x.BlockDiskStat)
                .Select(x => x.DiskVolKey)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            
            
            var fields = new[]
            {
                new {Field = "ReadSectors", GetField = (Func<BlockStatistics, long>) (row => row.ReadSectors)},
                new {Field = "ReadOperations", GetField = (Func<BlockStatistics, long>) (row => row.ReadOperations)},
                new {Field = "WriteSectors", GetField = (Func<BlockStatistics, long>) (row => row.WriteSectors)},
                new {Field = "WriteOperations", GetField = (Func<BlockStatistics, long>) (row => row.WriteOperations)},
                new {Field = "IoMilliseconds", GetField = (Func<BlockStatistics, long>) (row => row.IoMilliseconds)},
                new {Field = "InFlightRequests", GetField = (Func<BlockStatistics, long>) (row => row.InFlightRequests)},
            };

            /*
             * WAS:
             * 61: 51000, {1,104,817.299 = 1,101,895.193 [user] + 2,922.106 [kernel] milliseconds}
             * NOW:
             * 61: 51000, {649,414.066 = 647,727.807 [user] + 1,686.259 [kernel] milliseconds}
             */
            
            // blockName: PublicFastBlockMetrics
            Dictionary<string, PublicFastBlockMetrics> fastBlocksView = new Dictionary<string, PublicFastBlockMetrics>();
            foreach (BlockDiskDataSourcePoint atStat in dataSource) // over point in time
            {
                DateTime atDateTime = atStat.At;
                foreach (DiskVolStatModel blockRow in atStat.BlockDiskStat) // over disks
                {
                    var diskOrVolName = blockRow.DiskVolKey;
                    var publicFastBlockMetrics = fastBlocksView.GetOrAdd(diskOrVolName, _ => new PublicFastBlockMetrics());
                    publicFastBlockMetrics.Append(blockRow);

                    // var byBlock = blocksView.GetOrAdd(diskOrVolName, _ => new Dictionary<string, List<long>>());
                    // foreach (var fieldMetadata in fields) // over fields
                    // {
                    //    string fieldName = fieldMetadata.Field;
                    //    List<long> byField = byBlock.GetOrAdd(fieldName, _ => new List<long>());
                    //    long fieldValue = fieldMetadata.GetField(blockRow.Stat);
                    //    byField.Add(fieldValue);
                    // }
                }

                var missedBlockNames = blockNames.Except(atStat.BlockDiskStat.Select(x => x.DiskVolKey));
                foreach (var missedBlockName in missedBlockNames)
                {
                    var publicFastBlockMetrics = fastBlocksView.GetOrAdd(missedBlockName, _ => new PublicFastBlockMetrics());
                    publicFastBlockMetrics.AppendMissed();

                    // var byBlock = blocksView.GetOrAdd(missedBlockName, _ => new Dictionary<string, List<long>>());
                    // foreach (var fieldMetadata in fields)
                    // {
                    //   string fieldName = fieldMetadata.Field;
                    //   var byField = byBlock.GetOrAdd(fieldName, _ => new List<long>());
                    //   long fieldValue = byField.LastOrDefault();
                    //   byField.Add(fieldValue);
                    // }

                }
            }

            // blockName, FieldName, Y[]  
            Dictionary<string, Dictionary<string, List<long>>> blocksView = new Dictionary<string, Dictionary<string, List<long>>>();

            foreach (var fastPairs in fastBlocksView)
            {
                var blockOrVolName = fastPairs.Key;
                var publicFastBlockMetrics = fastPairs.Value;
                blocksView[blockOrVolName] = publicFastBlockMetrics.AsPublicView();
            }

            foreach (Dictionary<string,List<long>> byDisk in blocksView.Values)
            {
                foreach (KeyValuePair<string, List<long>> pair2 in byDisk)
                {
                    if (pair2.Key.EndsWith("Sectors"))
                    {
                        List<long> longs = pair2.Value;
                        for(int i=0;i<longs.Count; i++)
                        {
                            // AHAHA
                            longs[i] = longs[i] * 512;
                        }
                    }
                }
            }
            
            

            dynamic ret = new ExpandoObject();
            // ret.BlockNames = blockNames;
            ret.BlockNames = BlockDevicesUI.GetOrderedBlockNames(blockNames);
            ret.BlockTotals = BlockDiskDataSource.Instance.Totals;
            ret.Blocks = blocksView;
            return ret;

        }

        class PublicFastBlockMetrics
        {
            public List<long> ReadSectors = new List<long>(62);
            public List<long> ReadOperations = new List<long>(62);
            public List<long> WriteSectors = new List<long>(62);
            public List<long> WriteOperations = new List<long>(62);
            public List<long> IoMilliseconds = new List<long>(62);
            public List<long> InFlightRequests = new List<long>(62);

            public void Append(DiskVolStatModel blockRow)
            {
                ReadSectors.Add(blockRow.Stat.ReadSectors);
                ReadOperations.Add(blockRow.Stat.ReadOperations);
                WriteSectors.Add(blockRow.Stat.WriteSectors);
                WriteOperations.Add(blockRow.Stat.WriteOperations);
                IoMilliseconds.Add(blockRow.Stat.IoMilliseconds);
                InFlightRequests.Add(blockRow.Stat.InFlightRequests);
            }

            public void AppendMissed()
            {
                ReadSectors.Add(ReadSectors.LastOrDefault());
                ReadOperations.Add(ReadOperations.LastOrDefault());
                WriteSectors.Add(WriteSectors.LastOrDefault());
                WriteOperations.Add(WriteOperations.LastOrDefault());
                IoMilliseconds.Add(IoMilliseconds.LastOrDefault());
                InFlightRequests.Add(InFlightRequests.LastOrDefault());
            }

            public Dictionary<string, List<long>> AsPublicView()
            {
                Dictionary<string, List<long>> ret = new Dictionary<string, List<long>>();
                ret["ReadSectors"] = this.ReadSectors;
                ret["ReadOperations"] = this.ReadOperations;
                ret["WriteSectors"] = this.WriteSectors;
                ret["WriteOperations"] = this.WriteOperations;
                ret["IoMilliseconds"] = this.IoMilliseconds;
                ret["InFlightRequests"] = this.InFlightRequests;
                return ret;
            }
        }
    }
}