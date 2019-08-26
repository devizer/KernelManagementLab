using System;
using System.Diagnostics;
using System.Linq;
using SpeedTest;
using SpeedTest.Models;

namespace NetBenchmarkLab
{
    public class TestSpeed1
    {
        public static void Run()
        {
            SpeedTestClient client = new SpeedTestClient();
            Settings settings = client.GetSettings();

            string GetTitle(Server server)
            {
                return server.Sponsor + ", " + server.Country;
            }

            var servers = settings.Servers.Take(22).ToArray();
            int len = servers.Select(x => GetTitle(x).Length).Max();
            // Console.WriteLine($"Server: {server.Name}");
            for (int i = 0; i < servers.Length; i++)
            {
                Console.Write("Server: {0," + len + "}", GetTitle(servers[i]));
                for (int t = 0; t < 5; t++)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    var latency = client.TestServerLatency(servers[i], retryCount: 1);
                    Console.Write($" {latency,5}");
                }
                Console.WriteLine();
            }

        }

    }
}