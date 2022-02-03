using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualizeFioTestMatrix
{
    public class DataSource
    {
        public List<string> AllEngines { get; } 
        public List<string> AllArchs { get; } 
        public List<RawBenchmark> RawBenchmarkList { get; }

        public DataSource(List<RawBenchmark> rawBenchmarkList)
        {
            RawBenchmarkList = rawBenchmarkList;
            Console.WriteLine($"Total Runs: {RawBenchmarkList.Count}");

            AllEngines = RawBenchmarkList.Select(x => x.Engine).Distinct().ToList();
            Console.WriteLine($"All Engines: [{string.Join(",", AllEngines)}]");
            
            AllArchs = RawBenchmarkList.Select(x => x.Arch).Distinct().ToList();
            Console.WriteLine($"All Archs: [{string.Join(",", AllArchs)}]");
        }
    }
    
    public class RawBenchmark
    {
        public string Engine { get; set; }
        public int ExitCode { get; set; }
        public bool IsSuccess => ExitCode == 0;
        public string Arch { get; set; }
        public string OsAndVersion { get; set; }
        public string GLibCVersion { get; set; }
        public string ContainerName { get; set; }
        
        public string FioRaw { get; set; }
        public string Image { get; set; }
    }
}
