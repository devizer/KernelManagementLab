using KernelManagementJam.Benchmarks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Universe.Benchmark.DiskBench
{
    public class DiskBenchmarkOptions
    {
        public string WorkFolder { get; set; }
        public int StepDuration { get; set;}
        public long FileSize { get; set;}
        public int ThreadsNumber { get; set;}
        [JsonConverter(typeof(StringEnumConverter))]
        public DataGeneratorFlavour Flavour { get; set;}
        public int RandomAccessBlockSize { get; set;}
        public bool DisableODirect { get; set;}
    }
}