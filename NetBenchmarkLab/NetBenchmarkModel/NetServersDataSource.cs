using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using KernelManagementJam.DebugUtils;
using NetBenchmarkLab.NetBenchmarkModel;
using RegionsByTrafficPopularity;
using SpeedTest.Models;

namespace NetBenchmarkLab
{
    public class NetServersDataSource
    {
        private static readonly Dictionary<string, string> StateCodes = UsaStates.StateAbbreviations;
        public static dynamic Build(IEnumerable<Server> argServers)
        {
            var areas = RegionsByTrafficSource.Model.Areas;

            var servers = argServers.Select(x => x.ToServerModel()).ToArray();
            var indexById = argServers
                .Select((server, index) => new {server, index})
                .ToDictionary(x => x.server.Id, x => x.index);
                
            Dictionary<int, ServerModel> serversById = servers.ToDictionary(x => x.Id);
            dynamic ret = new ExpandoObject();
            ret.Areas = new List<dynamic>();
            ret.Servers = servers;
            
            var serversByCountry = servers.ToLookup(x => x.Country);
            ServerModel[] usaServers = servers.Where(s => s.Country == "United States").ToArray();

            StringBuilder serversByRegions = new StringBuilder();
            foreach (RegionsByTrafficModel.Area area in areas)
            {
                dynamic areaModel = new ExpandoObject();
                areaModel.AreaName = area.AreaName;
                areaModel.Regions = new List<dynamic>();
                ret.Areas.Add(areaModel);
                
                serversByRegions.AppendLine(area.AreaName);
                foreach (RegionsByTrafficModel.RegionInfo region in area.Regions)
                {
                    dynamic regionModel = new ExpandoObject();
                    regionModel.Name = region.Name;
                    regionModel.Fraction = region.Fraction;
                    areaModel.Regions.Add(regionModel);
                    
                    
                    // var foundServers = servers.Where(s => s.GetCountry() == region.Name);
                    List<ServerModel> foundServers = FindUsaServers(area,region,usaServers);
                    if (foundServers == null || foundServers.Count <= 0) foundServers = serversByCountry[region.Name].ToList();
                    // var cities = foundServers.Select(x => x.Name).Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(x => x).ToArray();
                    var cities = foundServers.Select(x => x.City)
                        .ToLookup(x => x, x => x, StringComparer.CurrentCultureIgnoreCase).OrderByDescending(x => x.Count()).Select(x => x.Key).ToArray();
                    
                    var reportRow = $"  {region.Name} [{region.Fraction:f0}%]: {foundServers.Count} servers, {cities.Length} cities ({string.Join("; ", cities)})";
                    regionModel.Description = reportRow.TrimStart();
                    // regionModel.ServersIds = string.Join(",", foundServers.Select(x => x.Id)); 
                    regionModel.ServersIndexes = string.Join(",", foundServers.Select(x => indexById[x.Id]));
                    // regionModel.Servers = foundServers.Select(x => x.Id).ToArray(); 
                    
                    serversByRegions.AppendLine(reportRow);
                }

                serversByRegions.AppendLine();
            }
            
            // Console.WriteLine(serversByRegions);
            DebugDumper.DumpText(serversByRegions.ToString(), "Net-Servers-Data-Source.txt");

            return ret;
        }
        
        static List<ServerModel> FindUsaServers(RegionsByTrafficModel.Area area, RegionsByTrafficModel.RegionInfo region, ServerModel[] usaServers)
        {
            if (area.AreaName == "America")
            {
                return usaServers
                    .Where(x => x.Country == "United States")
                    .Where(x => x.State != null && x.State.Equals(region.Name, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }

            // Not an America
            return null;
            
        }

        
 
    }
}