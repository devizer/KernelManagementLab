namespace Universe.FioStream
{
    // cpu          : usr=2.11%, sys=8.62%, ctx=10571, majf=0, minf=68
    // cpu          : usr=0.43%, sys=3.03%, ctx=6124, majf=0, minf=58
    // cpu          : usr=0.06%, sys=0.77%, ctx=474, majf=0, minf=1
    public class JobSummaryCpuUsage
    {
        public double UserPercents { get; set; }
        public double KernelPercents { get; set; }

        public override string ToString()
        {
            return $"{nameof(UserPercents)}: {UserPercents}, {nameof(KernelPercents)}: {KernelPercents}";
        }
    }
}