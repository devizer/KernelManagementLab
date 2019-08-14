using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KernelManagementJam.DebugUtils;
using NetBenchmarkLab.NetBenchmarkModel;
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
                    // var cities = foundServers.Select(x => x.Name).Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(x => x).ToArray();
                    var cities = foundServers.Select(x => x.Name)
                        .ToLookup(x => x, x => x, StringComparer.CurrentCultureIgnoreCase).OrderByDescending(x => x.Count()).Select(x => x.Key).ToArray();
                    
                    var reportRow = $"  {region.Name} [{region.Fraction:f0}]: {foundServers.Count} servers, {cities.Length} cities ({string.Join("; ", cities)})";
                    serversByRegions.AppendLine(reportRow);
                }

                serversByRegions.AppendLine();
            }
            
            Console.WriteLine(serversByRegions);
            DebugDumper.DumpText(serversByRegions.ToString(), "Servers-by-Regions.txt");
            
        }

        private static readonly Dictionary<string, string> StateCodes = UsaStates.StateAbbreviations;
        static List<Server> FindUsaServers_Legacy(RegionsByTrafficModel.Area area, RegionsByTrafficModel.RegionInfo region, Server[] usaServers)
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

        static List<Server> FindUsaServers(RegionsByTrafficModel.Area area, RegionsByTrafficModel.RegionInfo region, Server[] usaServers)
        {
            if (area.AreaName == "America")
            {
                List<Server> ret = new List<Server>();
                foreach (var usaServer in usaServers)
                {
                    bool isUsaCity = TryUsaCity(
                        usaServer.Country, usaServer.Name, out var city, out var stateName, out var stateCode
                    );

                    if (isUsaCity)
                    {
                        if (stateName.Equals(region.Name, StringComparison.InvariantCultureIgnoreCase))
                            ret.Add(usaServer);
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


        // San Francisco, CA -> { San Francisco; California; CA }  
        static bool TryUsaCity(string serverCountry, string serverCity, out string city, out string stateName, out string stateCode)
        {
            city = null;
            stateName = null;
            stateCode = null;
            bool ret = false;
            if (serverCountry == "United States")
            {
                var arr = serverCity.Split(',');
                if (arr.Length == 2)
                {
                    stateCode = arr[1].Trim();
                    if (StateCodes.TryGetValue(stateCode, out stateName))
                    {
                        ret = true;
                        city = arr[0].Trim();
                    }
                }
            }

            return ret;
        }
    }
    


}