using System;
using System.Collections.Generic;

namespace Universe.FioStream.Binaries
{
    public class FioEnginesProvider
    {
        private FioFeaturesCache FeaturesCache;

        public List<Engine> GetEngines()
        {
            throw new NotImplementedException();
        }
        
        public class Engine
        {
            public string Id { get; set; }
            public string Executable { get; set; }
            public Version Version { get; set; }
        }

    }
}