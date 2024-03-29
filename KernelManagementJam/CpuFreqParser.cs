using System;
using System.Collections.Generic;

namespace KernelManagementJam
{
    public class CpuFreqInfo
    {
        public long? Frequency { get; set; }
        public bool IsOnline { get; set; }

        public override string ToString()
        {
            return $"{nameof(IsOnline)}: {IsOnline}, {nameof(Frequency)}: {Frequency}";
        }
    }

    public class CpuFreqInfoParser
    {
        public static List<CpuFreqInfo> Parse()
        {
            const int MaxCpuCount = 256;
            List<CpuFreqInfo> ret = new List<CpuFreqInfo>();
            for (int i = 0; i < MaxCpuCount; i++)
            {
                // /sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq
                // /sys/devices/system/cpu/cpu1/online, except 0
                var nameOnline = $"/sys/devices/system/cpu/cpu{i}/online";
                string rawOnline = SmallFileReader.ReadFirstLine(nameOnline);
                bool isOnline = rawOnline != "0";
                if (i > 0 && rawOnline == null) break;
                var nameFreq = $"/sys/devices/system/cpu/cpu{i}/cpufreq/scaling_cur_freq";
                var rawFreq = SmallFileReader.ReadFirstLine(nameFreq);
                long? freq = null;
                if (rawFreq != null)
                {
                    if (long.TryParse(rawFreq, out var tempFreq))
                        freq = tempFreq;
                }
                
                ret.Add(new CpuFreqInfo() { Frequency = freq, IsOnline = isOnline});
            }

            return ret;
        }
    }

    public static class CpuFreqInfoExtensions
    {
        public static string ToShortHtmlInfo(this IEnumerable<CpuFreqInfo> cpuList)
        {
            long min = long.MaxValue, max = 0;
            foreach (var cpuFreqInfo in cpuList)
            {
                var freq = cpuFreqInfo.Frequency.GetValueOrDefault();
                if (freq > 0)
                {
                    min = Math.Min(min, freq);
                    max = Math.Max(max, freq);
                }
            }

            if (max == 0)
                return null;

            long scale = 1000000;
            string units = "GHz";
            string format = "f2";
            if (max < 1000000)
            {
                scale = 1000;
                units = "MHz";
                format = "f0";
            }

            var maxStr = (max / (double) scale).ToString(format);
            var minStr = (min / (double) scale).ToString(format);
            if (maxStr == minStr)
                return $"{maxStr} {units}";
            else 
                return $"{minStr} … {maxStr} {units}";
        }
    }
}