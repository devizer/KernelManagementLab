using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Universe.FioStream
{
    public class FioStreamReader
    {

        public class JobSummaryResult
        {
            public double Iops { get; set; }
            public double Bandwidth { get; set; }

            public override string ToString()
            {
                return $"{nameof(Iops)}: {Iops}, {nameof(Bandwidth)}: {Bandwidth}";
            }
        }
        
        public Action<JobSummaryResult> NotifyJobSummary { get; set; }

        public void ReadStreamToEnd(StreamReader streamReader)
        {
            while(true)
            {
                string line = streamReader.ReadLine();
                if (line != null)
                    ReadNextLine(line);
                else 
                    return;
            }
        }
        
        public void ReadNextLine(string line)
        {
            var lineLength = line.Length;
            var colonCharIndex = line.IndexOf(':');
            if (colonCharIndex <= 0 || colonCharIndex == lineLength - 1) return;
            // Progress - Jobs: ...
            // summary:
            //    read: IOPS=24.5k, BW=95.7MiB/s (100MB/s)(287MiB/3002msec) /* 3.6 */
            //    read : io=4288.0MB, bw=1416.6MB/s, iops=1416 , runt=  3027msec /* 2.0 */
            //    write: IOPS=22.9k, BW=89.4MiB/s (93.8MB/s)(269MiB/3003msec); 0 zone resets /* 3.26 */
            //    read: IOPS=136, BW=136MiB/s (143MB/s)(448MiB/3288msec)", /* 2.21 */
            var key1 = line.Substring(0, colonCharIndex);
            var value1 = line.Substring(colonCharIndex + 1);
            key1 = key1.Trim();
            if (key1.Equals("Jobs", StringComparison.InvariantCultureIgnoreCase))
            {
                var brakets = ReadBracketSections(value1).ToArray();
                Console.WriteLine($"PROGRESS ({brakets.Length} brakets): {string.Join("; ",brakets)}");
            }
            else if (key1.Equals("read", StringComparison.InvariantCultureIgnoreCase) || key1.Equals("write", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine($"SUMMARY: {value1}");
                var summaryPartsRaw = value1.Split(new[] {',', '('});
                double? bandwidth = null, iops = null;
                foreach (var summaryPartRaw in summaryPartsRaw)
                {
                    // IOPS=22.9k, iops=1111
                    // bw=BW=89.4MiB/s bw=1416.6MB/s
                    var pairRaw = summaryPartRaw.Split('=');
                    if (pairRaw.Length == 2)
                    {
                        var summaryPairKey = pairRaw[0].Trim();
                        var summaryPairValue = pairRaw[1].Trim();
                        bool isIops = summaryPairKey.Equals("iops", StringComparison.InvariantCultureIgnoreCase);
                        bool isBandwidth = !isIops && summaryPairKey.Equals("bw", StringComparison.InvariantCultureIgnoreCase);
                        if (isIops) 
                            iops = TryParseIops(summaryPairValue);
                        else if (isBandwidth) 
                            bandwidth = TryParseBandwidth(summaryPairValue);
                    }
                }

                if (iops.HasValue && bandwidth.HasValue)
                {
                    var notifyJobSummary = NotifyJobSummary;
                    if (notifyJobSummary != null)
                        NotifyJobSummary(new JobSummaryResult() {Bandwidth = bandwidth.Value, Iops = iops.Value});
                }
            }
        }

        private double? TryParseIops(string arg)
        {
            return TryParseHumanDouble(arg);
        }

        private double? TryParseBandwidth(string arg)
        {
            return TryParseHumanDouble(arg);
        }

        private static readonly CultureInfo EnUs = new CultureInfo("en-US");
        private double? TryParseHumanDouble(string arg)
        {
            var original = arg;
            
            if (arg.EndsWith("/s", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 3)
                arg = arg.Substring(0, arg.Length - 2);

            int scale = 1;
            if (arg.EndsWith("K", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 2)
            {
                arg = arg.Substring(0, arg.Length - 1);
                scale = 1024;
            }
            else if (arg.EndsWith("Kb", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 3)
            {
                arg = arg.Substring(0, arg.Length - 2);
                scale = 1024;
            }
            else if (arg.EndsWith("KiB", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 4)
            {
                arg = arg.Substring(0, arg.Length - 3);
                scale = 1024;
            }
            else if (arg.EndsWith("M", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 2)
            {
                arg = arg.Substring(0, arg.Length - 1);
                scale = 1024*1024;
            }
            else if (arg.EndsWith("Mb", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 3)
            {
                arg = arg.Substring(0, arg.Length - 2);
                scale = 1024*1024;
            }
            else if (arg.EndsWith("Mib", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 4)
            {
                arg = arg.Substring(0, arg.Length - 3);
                scale = 1024*1024;
            }
            else if (arg.EndsWith("G", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 2)
            {
                arg = arg.Substring(0, arg.Length - 1);
                scale = 1024*1024*1024;
            }
            else if (arg.EndsWith("Gb", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 3)
            {
                arg = arg.Substring(0, arg.Length - 2);
                scale = 1024*1024*1024;
            }
            else if (arg.EndsWith("Gib", StringComparison.InvariantCultureIgnoreCase) && arg.Length >= 4)
            {
                arg = arg.Substring(0, arg.Length - 3);
                scale = 1024*1024*1024;
            }

            if (double.TryParse(arg, NumberStyles.AllowDecimalPoint, EnUs, out var ret))
            {
                return ret * scale;
            }
            
#if DEBUG
            throw new ArgumentException($"Invalid bandwidth/IOPS argument [{original}]", arg);
#endif
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
    }
}