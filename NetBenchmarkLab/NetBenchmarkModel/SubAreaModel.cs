using System.Collections.Generic;
using NetBenchmarkLab.NetBenchmarkModel;

namespace NetBenchmarkLab
{
    public class SubAreaModel
    {
        // Sorted By Traffic
        public string Name { get; set; }
        
        // Sorted by number of servers
        public List<string> Cities { get; set; }
        
        // Non-Sorted
        public List<ServerModel> Servers { get; set; }
    }
}