using System;
using System.IO;
using Newtonsoft.Json;

namespace RegionsByTrafficPopularity
{
    public class RegionsByTrafficSource
    {
        private static Lazy<RegionsByTrafficModel> _Model = new Lazy<RegionsByTrafficModel>(() =>
        {
            var json = File.ReadAllText("regions-by-traffic.json");
            return JsonConvert.DeserializeObject<RegionsByTrafficModel>(json);
        });

        public static RegionsByTrafficModel Model => _Model.Value;
    }
}