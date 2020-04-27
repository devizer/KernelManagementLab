using System;
using System.IO;
using System.Linq;
using System.Text;

namespace KernelManagementJam
{
    // in KB
    public class LinuxMemorySummary
    {
        public long Total { get; set; }
        public long Free { get; set; }
        public long Available { get; set; }
        public long Buffers { get; set; }
        public long Cached { get; set; }
        public long SwapTotal { get; set; }
        public long SwapFree { get; set; }

        public override string ToString()
        {
            return $@"{nameof(Total)}: {Total:n0} KB, {nameof(Free)}: {Free:n0} KB, {nameof(Available)}: {Available:n0} KB, {nameof(Buffers)}: {Buffers:n0} KB, {nameof(Cached)}: {Cached:n0} KB, {nameof(SwapTotal)}: {SwapTotal:n0} KB, {nameof(SwapFree)}: {SwapFree:n0} KB";
        }

        public static bool TryParse(out LinuxMemorySummary info)
        {
            info = null;
            if (!File.Exists("/proc/meminfo"))
                return false;

            long? memTotal = null;
            long? memFree = null;
            long? memAvailable = null;
            long? buffers = null;
            long? cached = null;
            long? swapTotal = null;
            long? swapFree = null;

            StringComparer c = StringComparer.InvariantCultureIgnoreCase;
            using (FileStream fs = new FileStream("/proc/meminfo", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader rd = new StreamReader(fs, Encoding.UTF8))
            {
                string line;
                while ((line = rd.ReadLine()) != null)
                {
                    if (line.EndsWith(" Kb", StringComparison.InvariantCultureIgnoreCase))
                        line = line.Substring(0, line.Length - 3);

                    var arr = line.Split(':');
                    if (arr.Length != 2)
                        continue;


                    string rawKey = arr[0].Trim();
                    string rawVal = arr[1].Trim();
                    long val;
                    if (!long.TryParse(rawVal, out val))
                        continue;

                    if (c.Equals(rawKey, "memTotal")) memTotal = val;
                    else if (c.Equals(rawKey, "memFree")) memFree = val;
                    else if (c.Equals(rawKey, "memAvailable")) memAvailable = val;
                    else if (c.Equals(rawKey, "buffers")) buffers = val;
                    else if (c.Equals(rawKey, "cached")) cached = val;
                    else if (c.Equals(rawKey, "swapTotal")) swapTotal = val;
                    else if (c.Equals(rawKey, "swapFree")) swapFree = val;
                }
            }

            if (!memAvailable.HasValue)
            {
                memAvailable = memTotal.GetValueOrDefault() - memFree.GetValueOrDefault()
                               + buffers.GetValueOrDefault() + cached.GetValueOrDefault();
            }
            var all = new long?[] {memTotal, memFree, memAvailable, buffers, cached, swapTotal, swapFree};
            // Console.WriteLine("ALL: " + string.Join(", ", all.Select(x => Convert.ToString(x))));
            if (!all.All(x => x.HasValue))
                return false;

            info = new LinuxMemorySummary()
            {
                Total = memTotal.Value,
                Free = memFree.Value,
                Available = memAvailable.Value,
                Buffers = buffers.Value,
                Cached = cached.Value,
                SwapFree = swapFree.Value,
                SwapTotal = swapTotal.Value
            };

            // Trace.WriteLine("MemInfo_OnLinix: " + Environment.NewLine + "   " + info);

            return true;
        }

    }
}