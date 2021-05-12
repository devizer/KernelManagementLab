using System.Collections.Generic;

namespace Universe.FioStream.Binaries
{
    public class FioFeaturesCache
    {
        public IPicoLogger Logger { get; set; }

        private Dictionary<string, FioFeatures> Features = new Dictionary<string, FioFeatures>();

        public FioFeatures this[Candidates.Info candidate]
        {
            get
            {
                if (!Features.TryGetValue(candidate.Name, out var ret))
                {
                    if (candidate.Url == "skip://downloading")
                        ret = new FioFeatures(candidate.Name) {Logger = Logger};
                    else
                    {
                        // TODO: try and retry
                        GZipCachedDownloader d = new GZipCachedDownloader();
                        var cached = d.CacheGZip(candidate.Name, candidate.Url);
                        ret = new FioFeatures(cached) {Logger = Logger};
                    }

                    Features[candidate.Name] = ret;
                }

                return ret;
            }
        } 
    }
}