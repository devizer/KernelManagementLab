using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using KernelManagementJam;
using SpeedTest;
using SpeedTest.Models;

namespace NetBenchmarkLab
{
    public class ListServers
    {
        public static void Run()
        {
            var regionsByTraffic = RegionsByTrafficPopularity.RegionsByTrafficSource.Model;
            
            SpeedTestClient _speedTestClientClient = new SpeedTestClient();
            var settings = _speedTestClientClient.GetSettings();
            var ignoredIds = settings.ServerConfig.IgnoreIds.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            Server[] servers = settings.Servers.Where(s => !ignoredIds.Contains(s.Id.ToString(CultureInfo.InvariantCulture))).ToArray();
            
            var columns = new[] {
                "Id", "Name", "!", "Country", "Sponsor", "Host", "Distance", "Latency"
            };
            
            ConsoleTable report = new ConsoleTable(columns);
            Dictionary<string, List<object[]>> missedCountries = new Dictionary<string,List<object[]>>(StringComparer.InvariantCultureIgnoreCase);
            
                
            
            foreach (var server in servers)
            {
                string country = server.GetCountry();
                bool hasCountry = regionsByTraffic.Areas.SelectMany(x => x.Regions).Any(x => country.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase));
                
                

                var rowContent = new object[] {server.Id,  server.Name, 
                    hasCountry ? "+" : "", country, 
                    server.Sponsor, server.Host,
                    server.Distance.ToString("f1"), server.Latency};


                if (!hasCountry)
                {
                    if (!missedCountries.ContainsKey(country)) missedCountries[country] = new List<object[]>();
                    missedCountries[country].Add(rowContent);
                    // reportMissedCountries.AddRow(rowContent);
                }
                
                report.AddRow(rowContent);
            }
            
            Console.WriteLine($"REPORT{Environment.NewLine}{report}");
            File.WriteAllText("Servers.txt", report.ToString());
            
            ConsoleTable reportMissedCountries = new ConsoleTable(columns);
            foreach (var country in missedCountries.Keys.OrderByDescending(x => missedCountries[x].Count))
            {
                foreach (var row in missedCountries[country])
                {
                    reportMissedCountries.AddRow(row);
                }
            }

            File.WriteAllText("MissedCountries.txt", reportMissedCountries.ToString());
        }
    }
}