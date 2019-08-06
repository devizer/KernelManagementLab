using System;
using System.Collections.Generic;
using System.Linq;
using SpeedTest.Models;

namespace NetBenchmarkLab
{
    public static class SpeedTestExtensions
    {
        private static KeyValuePair<string, string>[] Countries = new[]
        {
            new KeyValuePair<string, string>("UA", "Ukraine"),
            new KeyValuePair<string, string>("PL", "Poland"),
            new KeyValuePair<string, string>("Polska", "Poland"), 
            new KeyValuePair<string, string>("ML", "Moldavia"), 
            new KeyValuePair<string, string>("Moldova", "Moldavia"),
            new KeyValuePair<string, string>("Republic of Moldova", "Moldavia"),
            new KeyValuePair<string, string>("SK", "Slovakia"), 
            new KeyValuePair<string, string>("Russia", "Russian Federation"),
            new KeyValuePair<string, string>("RU", "Russian Federation"),
            new KeyValuePair<string, string>("CZ", "Czech Republic"),
            new KeyValuePair<string, string>("DE", "Germany"),
            new KeyValuePair<string, string>("Great Britain", "United Kingdom"),
            new KeyValuePair<string, string>("Brasil", "Brazil"),
            new KeyValuePair<string, string>("BR", "Brazil"),
            new KeyValuePair<string, string>("US", "United States"),
            new KeyValuePair<string, string>("Trinidad", "Trinidad and Tobago"),
            new KeyValuePair<string, string>("LS", "Lesotho"),
            new KeyValuePair<string, string>("JP", "Japan"),
            new KeyValuePair<string, string>("IN", "India"), 
            
            new KeyValuePair<string, string>("ZA", "South Africa"), 
            new KeyValuePair<string, string>("DR Congo", "Democratic Republic of Congo"), 
            new KeyValuePair<string, string>("Congo, the Democratic Republic of the", "Democratic Republic of Congo"),
            new KeyValuePair<string, string>("Republic of the Union of Myanmar", "Myanmar"), 
            new KeyValuePair<string, string>("MM", "Myanmar"),
            
            new KeyValuePair<string, string>("MG", "Madagascar"),
            new KeyValuePair<string, string>("Netherland", "Netherlands"),
            
            new KeyValuePair<string, string>("CN", "China"),
            new KeyValuePair<string, string>("Viet Nam", "Vietnam"),
            new KeyValuePair<string, string>("Bosnia and Herzegovina", "Bosnia/Herzegovina"), 
            new KeyValuePair<string, string>("ES", "Spain"), 
            new KeyValuePair<string, string>("IT", "Italy"),
            new KeyValuePair<string, string>("AR", "Argentina"),
            
            new KeyValuePair<string, string>("Iran, Islamic Republic of", "Iran"),
            new KeyValuePair<string, string>("Republic of Singapore", "Singapore"),
            
            new KeyValuePair<string, string>("FJ", "Fiji"),
            new KeyValuePair<string, string>("CL", "Chile"), 
            
            
            
        };
    
        public static string GetCountry(this Server server)
        {
            var found = Countries.FirstOrDefault(x =>
                x.Key.Equals(server.Country, StringComparison.InvariantCultureIgnoreCase));

            return string.IsNullOrEmpty(found.Value) ? server.Country : found.Value;
        }
    }
}