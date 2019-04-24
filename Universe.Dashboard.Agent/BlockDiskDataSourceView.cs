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

            return totals
                .Select(x => x.DiskVolKey)
                .OrderBy(x => x)
                .ToList();
        }
        
        public static object AsViewModel(List<BlockDiskDataSourcePoint> dataSource)
        {
            var blockNames = dataSource
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
            
            // blockName, FieldName, Y[]  
            Dictionary<string, Dictionary<string, List<long>>> blocksView = new Dictionary<string, Dictionary<string, List<long>>>();
            int atPosition = 0;
            foreach (var atStat in dataSource)
            {
                atPosition++;
                foreach (var blockRow in atStat.BlockDiskStat)
                {
                    var diskOrVolName = blockRow.DiskVolKey;
                    var byBlock = blocksView.GetOrAdd(diskOrVolName, _ => new Dictionary<string, List<long>>());
                    foreach (var fieldMetadata in fields)
                    {
                        string fieldName = fieldMetadata.Field;
                        var byField = byBlock.GetOrAdd(fieldName, _ => new List<long>());
                        long fieldValue = fieldMetadata.GetField(blockRow.Stat);
                        byField.Add(fieldValue);
                    }
                }

                var missedBlockNames = blockNames.Except(atStat.BlockDiskStat.Select(x => x.DiskVolKey));
                foreach (var missedBlockName in missedBlockNames)
                {
                    var byBlock = blocksView.GetOrAdd(missedBlockName, _ => new Dictionary<string, List<long>>());
                    foreach (var fieldMetadata in fields)
                    {
                        string fieldName = fieldMetadata.Field;
                        var byField = byBlock.GetOrAdd(fieldName, _ => new List<long>());
                        long fieldValue = byField.LastOrDefault();
                        byField.Add(fieldValue);
                    }
                }
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
            ret.BlockNames = blockNames;
            ret.BlockTotals = BlockDiskDataSource.Instance.Totals;
            ret.Blocks = blocksView;
            return ret;

        }
    }
}