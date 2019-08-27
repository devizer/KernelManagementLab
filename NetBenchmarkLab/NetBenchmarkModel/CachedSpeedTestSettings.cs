using SpeedTest;
using SpeedTest.Models;

namespace NetBenchmarkLab.NetBenchmarkModel
{
    public class CachedSpeedTestSettings
    {
        private static readonly CachedInstance<Settings> _Settings = new CachedInstance<Settings>(
            600 * 1000,
            () => new SpeedTestClient().GetSettings()
        );
        
        private static CachedInstance<dynamic> _ServersDataSource = new CachedInstance<dynamic>(
            600*1000,
            () => NetServersDataSource.Build(Settings.Servers)); 

        public static Settings Settings => _Settings.Value;
        public static dynamic ServersDataSource => _ServersDataSource.Value;

    }
}