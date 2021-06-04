namespace Universe.FioStream
{
    public partial class FioStreamReader
    {
        
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


        private JobSummaryCpuUsage ParseJobSummaryCpuUsage(string rawValue)
        {
            var summaryPartsRaw = rawValue.Split(new[] {',', ' '});
            double? userPercents = null, kernelPercents = null;
            foreach (var summaryPartRaw in summaryPartsRaw)
            {
                // cpu          : usr=2.11%, sys=8.62%, ctx=10571, majf=0, minf=68
                // cpu          : usr=0.43%, sys=3.03%, ctx=6124, majf=0, minf=58
                // cpu          : usr=0.06%, sys=0.77%, ctx=474, majf=0, minf=1
                var pairRaw = summaryPartRaw.Split('=');
                if (pairRaw.Length == 2)
                {
                    var summaryPairKey = pairRaw[0];
                    var summaryPairValue = pairRaw[1];
                    bool isUser = summaryPairKey.Equals("usr", IgnoreCaseComparision);
                    bool isKernel = !isUser && summaryPairKey.Equals("sys", IgnoreCaseComparision);
                    if (isUser) 
                        userPercents = TryParsePercents(summaryPairValue);
                    else if (isKernel) 
                        kernelPercents = TryParsePercents(summaryPairValue);
                }
            }

            if (userPercents.HasValue && kernelPercents.HasValue)
                return new JobSummaryCpuUsage() {UserPercents = userPercents.Value, KernelPercents = kernelPercents.Value};

            return null;
        }
    }
}