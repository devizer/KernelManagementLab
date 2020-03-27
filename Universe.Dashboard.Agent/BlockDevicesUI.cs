using System;
using System.Collections.Generic;
using System.Linq;

namespace Universe.Dashboard.Agent
{
    public class BlockDevicesUI
    {
        public static List<string> GetOrderedBlockNames(List<string> blockNames)
        {
            if (blockNames == null) throw new ArgumentNullException(nameof(blockNames));
            
            var mounts = MountsDataSource.Mounts;

            var query = from block in blockNames
                join mount in mounts on $"/dev/{block}" equals mount.MountEntry.Device into gj
                from view in gj.DefaultIfEmpty()
                select new
                {
                    block = block,
                    mountPath = view?.MountEntry.MountPath,
                    totalSize = view?.TotalSize,
                };

            var ret = query
                .OrderBy(x => x.mountPath == "/" ? 0 : 1)
                .ThenByDescending(x => x.totalSize ?? 0)
                .ThenBy(x => x.block)
                .Select(x => x.block)
                .Distinct()
                .ToList();
            
 #if DEBUG
            Console.WriteLine($"SORTED BLOCK DEVICES: '{string.Join(",", ret)}'");
#endif

            return ret;
        }
    }
}