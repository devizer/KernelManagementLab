using System.Collections.Generic;

namespace RegionsByTrafficPopularity
{
    public class RegionsByTrafficModel
    {
        public class Area
        {
            public string AreaName { get; set; }
            public List<RegionInfo> Regions { get; set; }

            public Area()
            {
                Regions = new List<RegionInfo>();
            }
        }

        public class RegionInfo
        {
            public string Name { get; set; }
            public decimal Fraction { get; set; }
        }

        public List<Area> Areas { get; set; }

        public RegionsByTrafficModel()
        {
            Areas = new List<Area>();
        }
    }
}