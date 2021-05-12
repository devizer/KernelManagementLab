using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    public class Env
    {
        public static readonly FioFeaturesCache FeaturesCache = new FioFeaturesCache() {Logger = new PicoLogger()};
    }
}