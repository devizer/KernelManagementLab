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


        
    }
}