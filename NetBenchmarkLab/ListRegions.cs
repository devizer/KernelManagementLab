using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using RegionsByTrafficPopularity;
using SpeedTest;
using SpeedTest.Models;

namespace NetBenchmarkLab
{
    public class ListRegions
    {
        public static void Run()
        {
            var areas = RegionsByTrafficSource.Model.Areas;
            
            SpeedTestClient _speedTestClientClient = new SpeedTestClient();
            var settings = _speedTestClientClient.GetSettings();
            var ignoredIds = settings.ServerConfig.IgnoreIds.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            Server[] servers = settings.Servers.Where(s => !ignoredIds.Contains(s.Id.ToString(CultureInfo.InvariantCulture))).ToArray();
            var serversByCountry = servers.ToLookup(x => x.GetCountry());
            Server[] usaServers = servers.Where(s => s.Country == "United States").ToArray();

            StringBuilder serversByRegions = new StringBuilder();
            foreach (RegionsByTrafficModel.Area area in areas)
            {
                serversByRegions.AppendLine(area.AreaName);
                foreach (RegionsByTrafficModel.RegionInfo region in area.Regions)
                {
                    // var foundServers = servers.Where(s => s.GetCountry() == region.Name);
                    List<Server> foundServers = FindUsaServers(area,region,usaServers);
                    if (foundServers == null || foundServers.Count <= 0) foundServers = serversByCountry[region.Name].ToList();
                    var reportRow = $"  {region.Name} [{region.Fraction:f0}]: {foundServers.Count}";
                    serversByRegions.AppendLine(reportRow);
                }

                serversByRegions.AppendLine();
            }
            
            Console.WriteLine(serversByRegions);
            File.WriteAllText("Servers-by-Regions.txt", serversByRegions.ToString());
            
        }

        private static readonly Dictionary<string, string> StateCodes = UsaStates.StateAbbreviations;
        static List<Server> FindUsaServers(RegionsByTrafficModel.Area area, RegionsByTrafficModel.RegionInfo region, Server[] usaServers)
        {
            
            if (area.AreaName == "America")
            {
                List<Server> ret = new List<Server>();
                foreach (var usaServer in usaServers)
                {
                    var arr = usaServer.Name.Split(',');
                    if (arr.Length == 2)
                    {
                        var stateCode = arr[1].Trim();
                        if (StateCodes.TryGetValue(stateCode, out var stateName))
                        {
                            if (stateName.Equals(region.Name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                ret.Add(usaServer);
                            }
                        }
                    }
                }

                return ret;
            }
            else
            {
                // Not an America
                return null;
            }
        }
    }
}