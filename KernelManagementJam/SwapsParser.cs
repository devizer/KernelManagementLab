using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KernelManagementJam
{
    public class SwapsParser
    {

        public static List<SwapInfo> Parse()
        {
            return Parse("/proc/swaps");
        }

        public static List<SwapInfo> Parse(string fileName)
        {
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rd = new StreamReader(fs, new UTF8Encoding(false)))
            {
                return Parse(rd).ToList();
            }
        }
        
        public static IEnumerable<SwapInfo> Parse(TextReader reader)
        {
            string rawRow;
            while(true)
            {
                rawRow = reader.ReadLine();
                if (rawRow == null) break;
                
                string[] arr = rawRow.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length < 5) continue;
                string name = arr[0];
                string typeRaw = arr[1];
                SwapType type = SwapType.Other;
                if ("File".Equals(typeRaw, StringComparison.InvariantCultureIgnoreCase)) type = SwapType.File;
                else if ("Partition".Equals(typeRaw, StringComparison.InvariantCultureIgnoreCase)) type = SwapType.Partition;

                long size;
                if (!long.TryParse(arr[2], out size)) continue;

                long used;
                if (!long.TryParse(arr[3], out used)) continue;

                long priority;
                if (!long.TryParse(arr[4], out priority)) continue;

                yield return new SwapInfo()
                {
                    FileName = name,
                    Type = type,
                    TypeRaw = typeRaw,
                    Size = size,
                    Used = used,
                    Priority = priority,
                };
            }
        }
        
    }

    public class SwapInfo
    {
        public string FileName { get; set; }

        public SwapType Type { get; set; }
        public string TypeRaw { get; set; }

        // Mb
        public long Size { get; set; }
        public long Used { get; set; }
        
        // bigger - more 
        public long? Priority { get; set; }
    }

    public enum SwapType
    {
        Other,
        Partition,
        File,
    }
}