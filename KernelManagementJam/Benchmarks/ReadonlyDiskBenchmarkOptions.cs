using KernelManagementJam.Benchmarks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Universe.Benchmark.DiskBench
{
    public class ReadonlyDiskBenchmarkOptions_Obsolete
    {
        public string WorkFolder { get; set; }
        public int StepDuration { get; set;}
        public long WorkingSetSize { get; set;}
        public int ThreadsNumber { get; set;}
        public int RandomAccessBlockSize { get; set;}
        public bool DisableODirect { get; set;}
    }
}