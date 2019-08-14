using System;
using System.Collections.Generic;
using System.Linq;
using NetBenchmarkLab.NetBenchmarkModel;
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
            new KeyValuePair<string, string>("Ekaterinburg", "Russian Federation"), 
                
            new KeyValuePair<string, string>("CZ", "Czech Republic"),
            new KeyValuePair<string, string>("DE", "Germany"),
            new KeyValuePair<string, string>("Great Britain", "United Kingdom"),
            new KeyValuePair<string, string>("GB", "United Kingdom"),
            new KeyValuePair<string, string>("AU", "Australia"),
            new KeyValuePair<string, string>("FR", "France"),
            new KeyValuePair<string, string>("CA", "Canada"),
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
            new KeyValuePair<string, string>("VN", "Vietnam"), 
            new KeyValuePair<string, string>("Bosnia and Herzegovina", "Bosnia/Herzegovina"), 
            new KeyValuePair<string, string>("ES", "Spain"), 
            new KeyValuePair<string, string>("IT", "Italy"),
            new KeyValuePair<string, string>("AR", "Argentina"),
            
            new KeyValuePair<string, string>("Iran, Islamic Republic of", "Iran"),
            new KeyValuePair<string, string>("IR", "Iran"), 
            new KeyValuePair<string, string>("Republic of Singapore", "Singapore"),
            
            new KeyValuePair<string, string>("FJ", "Fiji"),
            new KeyValuePair<string, string>("CL", "Chile"),
            
            new KeyValuePair<string, string>("Republic of Maldives", "Maldives"),
            new KeyValuePair<string, string>("Phillipines", "Philippines"), 
        };
    
        public static string GetCountry(this Server server)
        {
            var found = Countries.FirstOrDefault(x =>
                x.Key.Equals(server.Country, StringComparison.InvariantCultureIgnoreCase));

            return string.IsNullOrEmpty(found.Value) ? server.Country : found.Value;
        }

        public static ServerModel ToServerModel(this Server server)
        {
            var serverCountry = server.GetCountry();
            if (!TryUsaCity(serverCountry, server.Name, out var city, out var stateName, out var stateCode))
            {
                city = server.Name;
                stateName = null;
                stateCode = null;
            }
            
            return new ServerModel
            {
                Id = server.Id,
                City = city,
                State = stateName,
                StateCode = stateCode,
                Country = serverCountry,
                Distance = Math.Round(server.Distance,1),
                Host = server.Host,
                Url = server.Url,
                Latency = server.Latency,
                Latitude = server.Latitude,
                Longitude = server.Longitude,
                Sponsor = server.Sponsor,
            };
        }
        
        private static readonly Dictionary<string, string> StateCodes = UsaStates.StateAbbreviations;
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