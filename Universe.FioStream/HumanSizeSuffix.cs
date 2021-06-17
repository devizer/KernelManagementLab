namespace Universe.FioStream
{
    internal class HumanSizeSuffix
    {
        public string Suffix { get; private set; }
        public int SuffixLength { get; private set; }
        public long Scale { get; private set; }

        private HumanSizeSuffix(string suffix, long scale)
        {
            Suffix = suffix;
            SuffixLength = suffix.Length;
            Scale = scale;
        }
            
        public static readonly HumanSizeSuffix[] All = new HumanSizeSuffix[]
        {
            new HumanSizeSuffix("K", 1024),
            new HumanSizeSuffix("KB", 1024),
            new HumanSizeSuffix("KiB", 1024),
            new HumanSizeSuffix("M", 1024*1024),
            new HumanSizeSuffix("MB", 1024*1024),
            new HumanSizeSuffix("MiB", 1024*1024),
            new HumanSizeSuffix("G", 1024*1024*1024),
            new HumanSizeSuffix("GB", 1024*1024*1024),
            new HumanSizeSuffix("GiB", 1024*1024*1024),
            new HumanSizeSuffix("T", 1024L*1024*1024*1024),
            new HumanSizeSuffix("TB", 1024L*1024*1024*1024),
            new HumanSizeSuffix("TiB", 1024L*1024*1024*1024),
            new HumanSizeSuffix("P", 1024L*1024*1024*1024*1024),
            new HumanSizeSuffix("PB", 1024L*1024*1024*1024*1024),
            new HumanSizeSuffix("PiB", 1024L*1024*1024*1024*1024),
            // should be THE LAST
            new HumanSizeSuffix("B", 1),
        };

    }
}