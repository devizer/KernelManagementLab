using System;
using System.ComponentModel.DataAnnotations.Schema;
using KernelManagementJam.DebugUtils;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Universe.Benchmark.DiskBench;
using Universe.DiskBench;

namespace Universe.Dashboard.DAL
{
    [Table("W3Top_DiskBenchmark")]
    public class DiskBenchmarkEntity
    {
        public int Id { get; set; }
        public Guid Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MountPath { get; set; }
        public DiskBenchmarkOptions Args { get; set; }
        public ProgressInfo Report { get; set; }
        
        public bool IsSuccess { get; set; }
        public string ErrorInfo { get; set; }
        public DiskbenchmarkEnvironment Environment { get; set; }
    }

    public class DiskbenchmarkEnvironment
    {
        public string FileSystems { get; set; }
        public string Engine { get; set; }
        public string EngineVersion { get; set; }
    }
}