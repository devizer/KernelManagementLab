namespace Universe.FioStream
{
    public class JobSummaryResult
    {
        public double Iops { get; set; }
        public double Bandwidth { get; set; }

        public override string ToString()
        {
            return $"{nameof(Iops)}: {Iops:n0}, {nameof(Bandwidth)}: {Bandwidth:n0}";
        }
    }
}