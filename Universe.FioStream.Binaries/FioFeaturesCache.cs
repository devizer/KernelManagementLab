using System.Collections.Concurrent;

namespace Universe.FioStream.Binaries
{
    public class FioFeaturesCache
    {
        public IPicoLogger Logger { get; set; }

        private ConcurrentDictionary<string, FioFeatures> Features = new ConcurrentDictionary<string, FioFeatures>();

        public FioFeatures this[Candidates.Info candidate]
        {
            get
            {
                if (!Features.TryGetValue(candidate.Name, out var ret))
                {
                    if (candidate.Url == Candidates.Info.LocalFioFakeUrl)
                        ret = new FioFeatures(candidate.Name) {Logger = Logger};
                    
                    else
                    {
                        // TODO: try and retry
                        GZipCachedDownloader d = new GZipCachedDownloader() { Logger = Logger};
                        var cachedBinary = d.CacheGZip(candidate.Name, candidate.Url);
                        ret = new FioFeatures(cachedBinary) {Logger = Logger};
                    }

                    Features[candidate.Name] = ret;
                }

                return ret;
            }
        } 
    }
}
