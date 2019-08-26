using System.Diagnostics;
using SpeedTest;

namespace NetBenchmarkLab.NetBenchmarkModel
{
    public class CachedSpeedTestSettings
    {
        private const long TTL = 600 * 1000; 
        private Stopwatch Stopwatch = null;
        private SpeedTest.Models.Settings _Settings;
        static readonly object Sync = new object(); 
        
        public static readonly CachedSpeedTestSettings Instance = new CachedSpeedTestSettings();

        public SpeedTest.Models.Settings Settings
        {
            get
            {
                if (_Settings != null && Stopwatch != null && Stopwatch.ElapsedMilliseconds <= TTL)
                    return _Settings;

                var settings = new SpeedTestClient().GetSettings();
                lock (Sync)
                {
                    if (_Settings != null) return _Settings;
                    Stopwatch = Stopwatch ?? Stopwatch.StartNew();
                    Stopwatch.Restart();
                    _Settings = settings;
                    return _Settings;
                }
            }
        }
    }
}