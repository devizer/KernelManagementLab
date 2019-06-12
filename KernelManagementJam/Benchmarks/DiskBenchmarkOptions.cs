using KernelManagementJam.Benchmarks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Universe.Benchmark.DiskBench
{
    public class DiskBenchmarkOptions
    {
        public string WorkFolder { get; set; }
        public int StepDuration { get; set;}
        public long WorkingSetSize { get; set;}
        public int ThreadsNumber { get; set;}
        public int RandomAccessBlockSize { get; set;}
        public bool DisableODirect { get; set;}
        
        // Ignored by ReadonlyDiskBenchmark
        [JsonConverter(typeof(StringEnumConverter))]
        public DataGeneratorFlavour Flavour { get; set;}

    }
}