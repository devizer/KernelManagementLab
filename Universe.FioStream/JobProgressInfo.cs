using System;

namespace Universe.FioStream
{
    public class JobProgressInfo
    {
        public ProgressStage? Stage { get; set; }
        public double? PerCents { get; set; }
        public double? ReadIops { get; set; }
        public double? ReadBandwidth { get; set; }
        public double? WriteIops { get; set; }
        public double? WriteBandwidth { get; set; }
        public TimeSpan? Eta { get; set; }

        public override string ToString()
        {
            return $"{nameof(Stage)}: [{Stage}], {nameof(PerCents)}: [{PerCents}], {nameof(ReadIops)}: [{ReadIops}], {nameof(ReadBandwidth)}: [{ReadBandwidth}], {nameof(WriteIops)}: [{WriteIops}], {nameof(WriteBandwidth)}: [{WriteBandwidth}], {nameof(Eta)}: [{Eta}]";
        }
    }
}