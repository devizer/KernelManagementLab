﻿using System;
using System.Collections.Generic;
using KernelManagementJam.DebugUtils;
using NetBenchmarkLab.NetBenchmarkModel;
using Newtonsoft.Json;
using SpeedTest;

namespace NetBenchmarkLab
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("DUMPS_ARE_ENABLED", "True");
            
            HellOfDictionary();
            
            ListServers.Run();
            
            ListRegions.Run();
            
            SaveServersDataSourceAsJson();
            
            TestSpeed1.Run();
        }

        private static void SaveServersDataSourceAsJson()
        {
            AdvancedSpeedTestClient _speedTestClientClient = new AdvancedSpeedTestClient();
            // var settings = _speedTestClientClient.GetSettings();
            var settings = CachedSpeedTestSettings.Settings;
            var dataSource = CachedSpeedTestSettings.ServersDataSource;
            DebugDumper.Dump(dataSource, "Net-Servers-Data-Source.json", minify: false);
            DebugDumper.Dump(dataSource, "Net-Servers-Data-Source.min.json", minify: true);
        }

        private static void HellOfDictionary()
        {
            Dictionary<string, int> points = new Dictionary<string, int>
            {
                {"Jo", 3474},
                {"Jess", 11926},
                {"James", 9001},
            };

            Dictionary<int, string> points2 = new Dictionary<int, string>
            {
                {55, "Jo"},
                {777, "Jess"},
                {1, "James"},
            };

            points2.Add(77, "XXXX");
            string json = JsonConvert.SerializeObject(points, Formatting.Indented);
            string json2 = JsonConvert.SerializeObject(points2, Formatting.Indented);
            Console.WriteLine(json);
            Console.WriteLine(json2);
        }

    }
}