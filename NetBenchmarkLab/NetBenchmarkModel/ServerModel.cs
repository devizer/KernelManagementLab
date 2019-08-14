using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SpeedTest.Models;

namespace NetBenchmarkLab.NetBenchmarkModel
{
    public class ServerModel
    {
        public int Id { get; set; }

        // Name -> City
        public string City { get; set; }
        
        // For USA only?
        public string State { get; set; }
        public string StateCode { get; set; }

        public string Country { get; set; }

        public string Sponsor { get; set; }

        public string Host { get; set; }

        public string Url { get; set; }

        [JsonProperty("lat")] public double Latitude { get; set; }

        [JsonProperty("lon")] public double Longitude { get; set; }

        public double Distance { get; set; }

        [JsonIgnore]
        public int Latency { get; set; }

    }
}