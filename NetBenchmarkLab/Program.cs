using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetBenchmarkLab
{
    class Program
    {
        static void Main(string[] args)
        {
            HellOfDictionary();
            
            ListServers.Run();
            
            ListRegions.Run();
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