using System;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    public class Env
    {
        public static readonly FioFeaturesCache FeaturesCache = new FioFeaturesCache() {Logger = new PicoLogger()};

        public static bool ShortFioTests
        {
            get
            {
                var raw = Environment.GetEnvironmentVariable("SHORT_FIO_TESTS");
                return string.Compare("True", raw, StringComparison.OrdinalIgnoreCase) == 0;
            }
        }
    }
}