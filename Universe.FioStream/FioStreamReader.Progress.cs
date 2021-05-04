using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Universe.FioStream
{
    public partial class FioStreamReader
    {
        
        // LENGTH - 1
        // [95252K/0K /s] [23.3K/0  iops] [eta 00m:01s]", /* 2.0 */
        // [62426KB/0KB/0KB /s] [60/0/0 iops] [eta 00m:00s]", /* 2.2.13 */
        // [97718KB/0KB/0KB /s] [24.5K/0/0 iops] [eta 00m:00s]", /* 2.7 */
        // [r=0KiB/s,w=115MiB/s][r=0,w=115 IOPS][eta 00m:00s]", /* 2.21 */
        // [r=0KiB/s,w=125MiB/s][r=0,w=125 IOPS][eta 00m:01s]", /* 2.99 */
        // [r=92.2MiB/s,w=0KiB/s][r=23.6k,w=0 IOPS][eta 00m:00s]", /* 3.0 */
        // [r=96.9MiB/s][r=24.8k IOPS][eta 00m:01s]", /* 3.12 */
        // [w=91.7MiB/s][w=23.5k IOPS][eta 00m:01s]", /* 3.16 */
        // [w=18.6MiB/s][w=18 IOPS][eta 00m:02s]", /* 3.26 */
        // [r=95.2MiB/s,w=0KiB/s][r=24.4k,w=0 IOPS][eta 00m:01s]", /* 3.6 */
        bool TryParseProgressIops(string raw, out double? iopsRead, out double? iopsWrite)
        {
            if (raw.Length >= 6 && raw.EndsWith(" iops", IgnoreCaseComparision))
            {
                raw = raw.Substring(0, raw.Length - 5);
            }
            else
            {
                iopsRead = iopsWrite = null;
                return false;
            }

            // both Bandwidth and iops
            var commaArray = raw.Split(',');
            bool isCommaKind = commaArray.Length == 1 || commaArray.Length == 2;

            if (isCommaKind)
            {
                iopsRead = iopsWrite = null;
                var d = ParseCommaSeparatedDictionary(commaArray, false);
                double val;
                if (d.TryGetValue("r", out val)) iopsRead = val;
                if (d.TryGetValue("w", out val)) iopsWrite = val;
                if (iopsRead.HasValue || iopsWrite.HasValue) 
                    return true;
            }
            
            var slashArray = raw.Split('/');
            bool isSlashedKind = slashArray.Length == 2 || slashArray.Length == 3;
            if (isSlashedKind)
            {
                iopsRead = iopsWrite = null;
            }

            iopsRead = iopsWrite = null;
            return false;
        }

        Dictionary<string, double> ParseCommaSeparatedDictionary(string[] array, bool isBandwidth)
        {
            Dictionary<string, double> ret = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in array)
            {
                int p = part.IndexOf('=');
                if (p > 0 && p < part.Length - 1)
                {
                    var key = part.Substring(0, p);
                    var value = part.Substring(p + 1);
                    var num = isBandwidth ? TryParseBandwidth(value) : TryParseIops(value);
                    if (num.HasValue) ret[key] = num.Value;
                }
            }

            return ret;
        }
        
        // LENGTH - 4
        // not used
        static ProgressStage? TryParseProgressStage(string raw)
        {
            if (raw.StartsWith("R", StringComparison.Ordinal))
                return ProgressStage.SeqRead;

            if (raw.StartsWith("r", StringComparison.Ordinal))
                return ProgressStage.RandRead;

            if (raw.StartsWith("W", StringComparison.Ordinal))
                return ProgressStage.SeqWrite;

            if (raw.StartsWith("w", StringComparison.Ordinal))
                return ProgressStage.RandWrite;

            return null;
        }
        
        // Length - 3
        //  "Jobs: 1 (f=1): [W] [66.7% done] [0K/39819K /s] [0 /37  iops] [eta 00m:02s]", /* 2.0 */
        //  "Jobs: 1 (f=1): [w(1)][83.3%][r=0KiB/s,w=86.0MiB/s][r=0,w=22.0k IOPS][eta 00m:01s]", /* 2.21 */
        static double? ParsePerCents(string raw)
        {
            if (raw.Length >= 6 && raw.EndsWith(" done", IgnoreCaseComparision))
            {
                raw = raw.Substring(0, raw.Length - 5);
            }

            if (!raw.EndsWith("%", IgnoreCaseComparision))
                return null;

            if (raw.Length <= 1) return null;
            raw = raw.Substring(0, raw.Length - 1);

            if (!double.TryParse(raw, NumberStyles.AllowDecimalPoint, EnUs, out var ret))
                return null;

            return ret;
        }
        
        static IEnumerable<string> ReadBracketSections(string raw)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in raw)
            {
                if (c == '[') sb.Clear();
                else if (c == ']')
                {
                    yield return sb.ToString();
                }
                else
                    sb.Append(c);
            }
        }
        
        // Converts to seconds from: [eta 01d:03h:46m:34s] [eta 115d:17h:46m:42s]
        static long? ParseEta(string arg)
        {
            if (arg.StartsWith("eta ", IgnoreCaseComparision) && arg.Length > 4)
                arg = arg.Substring(4);

            var parts = arg.Split(':');
            long? totalSeconds = null;
            foreach (var part in parts)
            {
                int scale = 1;
                var partLen = part.Length;
                string rawNumber = part;
                if (partLen > 1)
                {
                    char lastChar = part[partLen - 1];
                    if (lastChar == 's' || lastChar == 'S')
                    {
                        rawNumber = part.Substring(0, partLen - 1);
                        scale = 1;
                    }
                    else if (lastChar == 'm' || lastChar == 'M')
                    {
                        rawNumber = part.Substring(0, partLen - 1);
                        scale = 60;
                    } 
                    else if (lastChar == 'h' || lastChar == 'H')
                    {
                        rawNumber = part.Substring(0, partLen - 1);
                        scale = 60*60;
                    } 
                    else if (lastChar == 'd' || lastChar == 'D')
                    {
                        rawNumber = part.Substring(0, partLen - 1);
                        scale = 60*60*24;
                    } 
                }

                if (!long.TryParse(rawNumber, NumberStyles.Number, EnUs, out var number))
                {
#if DEBUG
                    throw new ArgumentException($"Unknown part '{part}' of ETA string '{arg}'", arg);
#endif
                }
                else
                {
                    totalSeconds = totalSeconds.GetValueOrDefault() + number * scale;
                }
            }

            return totalSeconds;
        }

        
    }
}