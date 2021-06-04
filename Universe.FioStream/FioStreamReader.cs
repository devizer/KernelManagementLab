using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Universe.FioStream
{
    
    public partial class FioStreamReader
    {
        public static bool ConsolasDebug = false;
        public Action<TimeSpan> NotifyEta { get; set; }
        public Action<JobSummaryCpuUsage> NotifyJobSummaryCpuUsage { get; set; }
        public Action<JobSummaryResult> NotifyJobSummary { get; set; }
        public Action<JobProgressInfo> NotifyJobProgress { get; set; }

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

        private bool IsFirstLine = true;
        public void ReadNextLine(string line)
        {
            var lineLength = line.Length;
            if (lineLength == 0) return;
            if (IsFirstLine)
            {
                
            }

            IsFirstLine = false;
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
                string[] brakets = ReadBracketSections(value1).ToArray();
                if (ConsolasDebug)
                    if (brakets.Length > 0)
                        Console.WriteLine($"PROGRESS ({brakets.Length} brakets): {string.Join("; ",brakets.Select(x => $"{{{x}}}").ToArray())}");
                
                if (brakets.Length >= 3)
                {
                    long? eta = ParseEta(brakets[brakets.Length - 1]);
                    if (eta != null)
                    {
                        var notifyEta = NotifyEta;
                        if (notifyEta != null)
                            notifyEta(TimeSpan.FromSeconds(eta.Value));
                    }
                    
                    if (brakets.Length >= 5)
                    {
                        JobProgressInfo jobProgressInfo = new JobProgressInfo
                        {
                            PerCents = ParsePerCents(brakets[brakets.Length - 4]),
                            Stage = TryParseProgressStage(brakets[brakets.Length - 5]),
                            Eta = eta.HasValue ? TimeSpan.FromSeconds(eta.Value) : (TimeSpan?) null,
                        };
                        
                        // IOPS
                        bool isIopsOk = TryParseProgressIops(brakets[brakets.Length - 2],out var iopsRead,out var iopsWrite); 
                        if (isIopsOk)
                        {
                            jobProgressInfo.ReadIops = iopsRead;
                            jobProgressInfo.WriteIops = iopsWrite;
                        }
                        
                        // Bandwidth
                        bool isBandwidthOk = TryParseProgressBandwidth(brakets[brakets.Length - 3], out var bandwidthRead, out var bandwidthWrite);
                        if (isBandwidthOk)
                        {
                            jobProgressInfo.ReadBandwidth = bandwidthRead;
                            jobProgressInfo.WriteBandwidth = bandwidthWrite;
                        }

                        bool hasIops = jobProgressInfo.ReadIops.GetValueOrDefault() > 0 ||
                                       jobProgressInfo.WriteIops.GetValueOrDefault() > 0;
                        
                        bool hasBandwidth = jobProgressInfo.ReadBandwidth.GetValueOrDefault() > 0 ||
                                            jobProgressInfo.WriteBandwidth.GetValueOrDefault() > 0;
                        
                        bool isWorkingStage = jobProgressInfo.Stage.HasValue && jobProgressInfo.Stage != ProgressStage.Heating;
                        if (/*hasBandwidth &&*/ hasIops && isWorkingStage)
                            NotifyJobProgress?.Invoke(jobProgressInfo);
                    }
                }

            }
            else if (key1.Equals("read", IgnoreCaseComparision) || key1.Equals("write", IgnoreCaseComparision))
            {
                // SUMMARY only if contains both 'iops' and 'bw'
                if (ConsolasDebug)
                    Console.WriteLine($"SUMMARY: {value1}");
                
                JobSummaryResult jobSummaryResult = ParseJobSummary(value1);
                if (jobSummaryResult != null)
                {
                    NotifyJobSummary?.Invoke(jobSummaryResult);
                }
            }
            else if (key1.Equals("cpu", IgnoreCaseComparision))
            {
                if (ConsolasDebug)
                    Console.WriteLine($"SUMMARY CPU USAGE: {value1}");

                var jobSummaryCpuUsage = ParseJobSummaryCpuUsage(value1);
                if (jobSummaryCpuUsage != null)
                {
                    NotifyJobSummaryCpuUsage?.Invoke(jobSummaryCpuUsage);
                }

            }
        }
        
    }
}