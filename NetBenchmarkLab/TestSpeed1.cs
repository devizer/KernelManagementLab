using System;
using System.Diagnostics;
using System.Linq;
using KernelManagementJam;
using NetBenchmarkLab.NetBenchmarkModel;
using SpeedTest;
using SpeedTest.Models;

namespace NetBenchmarkLab
{
    public class TestSpeed1
    {
        public static void Run()
        {
            SpeedTestClient client = new SpeedTestClient();
            Settings settings = CachedSpeedTestSettings.Settings;

            string GetTitle(Server server)
            {
                return $"{server.Sponsor} ({server.Country}) {server.Name}";
            }

            var servers = settings.Servers.Take(999).ToArray();
            int len = servers.Select(x => GetTitle(x).Length).Max();
            // Console.WriteLine($"Server: {server.Name}");
            for (int i = 0; i < servers.Length; i++)
            {
                Stopwatch swError = Stopwatch.StartNew();
                Console.Write($"{i,2}: {{0,{len}}}", GetTitle(servers[i]));
                try
                {
                    for (int t = 0; t < 5; t++)
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        var latency = client.TestServerLatency(servers[i], retryCount: 1);
                        Console.Write($" {latency,5}");
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.GetExceptionDigest() + " " + swError.Elapsed);
                }
                Console.WriteLine();
            }

        }

    }
}