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

        public class JobProgress
        {
            public ProgressStage? Stage { get; set; }
            public double? ReadIops { get; set; }
            public double? ReadBandwidth { get; set; }
            public double? WriteIops { get; set; }
            public double? WriteBandwidth { get; set; }

            public override string ToString()
            {
                return $"{nameof(Stage)}: {Stage}, {nameof(ReadIops)}: {ReadIops}, {nameof(ReadBandwidth)}: {ReadBandwidth}, {nameof(WriteIops)}: {WriteIops}, {nameof(WriteBandwidth)}: {WriteBandwidth}";
            }
        }

        public enum ProgressStage
        {
            SeqRead,
            SeqWrite,
            RandRead,
            RandWrite,
            Heating,
        }
        
        public Action<TimeSpan> NotifyEta { get; set; }
        public Action<JobSummaryResult> NotifyJobSummary { get; set; }
        public Action<JobProgress> NotifyJobProgress { get; set; }

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
            if (key1.Equals("Jobs", IgnoreCaseComparision))
            {
                // PROGRESS
                var brakets = ReadBracketSections(value1).ToArray();
#if DEBUG
                Console.WriteLine($"PROGRESS ({brakets.Length} brakets): {string.Join("; ",brakets.Select(x => $"{{{x}}}").ToArray())}");
#endif
                if (brakets.Length >= 3)
                {
                    long? eta = ParseEta(brakets[brakets.Length - 1]);
                    if (eta != null)
                    {
                        var notifyEta = NotifyEta;
                        if (notifyEta != null)
                            notifyEta(TimeSpan.FromSeconds(eta.Value));
                    }
                }
            }
            else if (key1.Equals("read", IgnoreCaseComparision) || key1.Equals("write", IgnoreCaseComparision))
            {
                // SUMMARY only if contains both 'iops' and 'bw'
#if DEBUG
                Console.WriteLine($"SUMMARY: {value1}");
#endif
                var jobSummaryResult = ParseJobSummary(value1);
                if (jobSummaryResult != null)
                {
                    var notifyJobSummary = NotifyJobSummary;
                    if (notifyJobSummary != null)
                        NotifyJobSummary(jobSummaryResult);
                }
            }
        }

        // summary:
        //    read: IOPS=24.5k, BW=95.7MiB/s (100MB/s)(287MiB/3002msec) /* 3.6 */
        //    read : io=4288.0MB, bw=1416.6MB/s, iops=1416 , runt=  3027msec /* 2.0 */
        //    write: IOPS=22.9k, BW=89.4MiB/s (93.8MB/s)(269MiB/3003msec); 0 zone resets /* 3.26 */
        //    read: IOPS=136, BW=136MiB/s (143MB/s)(448MiB/3288msec)", /* 2.21 */
        // argument goes after first colon
        static JobSummaryResult ParseJobSummary(string rawValue)
        {
            var summaryPartsRaw = rawValue.Split(new[] {',', '('});
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
                    bool isIops = summaryPairKey.Equals("iops", IgnoreCaseComparision);
                    bool isBandwidth = !isIops && summaryPairKey.Equals("bw", IgnoreCaseComparision);
                    if (isIops) 
                        iops = TryParseIops(summaryPairValue);
                    else if (isBandwidth) 
                        bandwidth = TryParseBandwidth(summaryPairValue);
                }
            }

            if (iops.HasValue && bandwidth.HasValue)
            { 
                return new JobSummaryResult() {Bandwidth = bandwidth.Value, Iops = iops.Value};
            }

            return null;
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

        private static double? TryParseIops(string arg)
        {
            return TryParseHumanDouble(arg);
        }

        private static double? TryParseBandwidth(string arg)
        {
            return TryParseHumanDouble(arg);
        }

        private static readonly CultureInfo EnUs = new CultureInfo("en-US");
        
        private static readonly StringComparison IgnoreCaseComparision = 
#if NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_6 || NETCOREAPP1_1 || NETCOREAPP1_0  
            StringComparison.OrdinalIgnoreCase;
#else
            StringComparison.InvariantCultureIgnoreCase;
#endif

        private static double? TryParseHumanDouble(string arg)
        {
            var original = arg;
            
            if (arg.EndsWith("/s", IgnoreCaseComparision) && arg.Length >= 3)
                arg = arg.Substring(0, arg.Length - 2);

            int scale = 1;
            if (arg.EndsWith("K", IgnoreCaseComparision) && arg.Length >= 2)
            {
                arg = arg.Substring(0, arg.Length - 1);
                scale = 1024;
            }
            else if (arg.EndsWith("Kb", IgnoreCaseComparision) && arg.Length >= 3)
            {
                arg = arg.Substring(0, arg.Length - 2);
                scale = 1024;
            }
            else if (arg.EndsWith("KiB", IgnoreCaseComparision) && arg.Length >= 4)
            {
                arg = arg.Substring(0, arg.Length - 3);
                scale = 1024;
            }
            else if (arg.EndsWith("M", IgnoreCaseComparision) && arg.Length >= 2)
            {
                arg = arg.Substring(0, arg.Length - 1);
                scale = 1024*1024;
            }
            else if (arg.EndsWith("Mb", IgnoreCaseComparision) && arg.Length >= 3)
            {
                arg = arg.Substring(0, arg.Length - 2);
                scale = 1024*1024;
            }
            else if (arg.EndsWith("Mib", IgnoreCaseComparision) && arg.Length >= 4)
            {
                arg = arg.Substring(0, arg.Length - 3);
                scale = 1024*1024;
            }
            else if (arg.EndsWith("G", IgnoreCaseComparision) && arg.Length >= 2)
            {
                arg = arg.Substring(0, arg.Length - 1);
                scale = 1024*1024*1024;
            }
            else if (arg.EndsWith("Gb", IgnoreCaseComparision) && arg.Length >= 3)
            {
                arg = arg.Substring(0, arg.Length - 2);
                scale = 1024*1024*1024;
            }
            else if (arg.EndsWith("Gib", IgnoreCaseComparision) && arg.Length >= 4)
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