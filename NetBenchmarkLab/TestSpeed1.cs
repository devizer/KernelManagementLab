using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
            AdvancedSpeedTestClient client = new AdvancedSpeedTestClient();
            Settings settings = CachedSpeedTestSettings.Settings;

            string GetTitle(Server server)
            {
                return $"{server.Sponsor} ({server.Country}) {server.Name} {{{server.Distance/1000:n0} km}}";
            }

            var servers = settings.Servers.Take(9999).ToArray();
            int len = servers.Select(x => GetTitle(x).Length).Max();
            // Console.WriteLine($"Server: {server.Name}");
            List<ServerLatency> latencyLog = new List<ServerLatency>();
            for (int i = 0; i < servers.Length; i++)
            {
                Stopwatch swError = Stopwatch.StartNew();
                StringBuilder reportRow = new StringBuilder(string.Format($"{i,2}: {{0,{len}}}", GetTitle(servers[i])));
                Console.Write(reportRow);
                try
                {
                    double latency = 42*42*42*42d;
                    for (int t = 0; t < 5; t++)
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        var latency1 = client.TestServerLatency(servers[i], retryCount: 1);
                        var latency2 = client.TestServerCorrectLatency(servers[i], 1);
                        var column = $" {latency1.ToString("f0")+"/" + latency2.ToString("f2"),13}";
                        reportRow.Append(column);
                        Console.Write(column);
                        latency = Math.Min(latency, latency2);
                    }

                    latencyLog.Add(new ServerLatency
                    {
                        LogLine = reportRow.ToString(),
                        Latency = latency,
                        Server = servers[i]
                    });
                    
                }
                catch (Exception ex)
                {
                    Console.Write(" " + ex.GetExceptionDigest() + " " + swError.Elapsed);
                }
                Console.WriteLine();

                if (i % 12 == 0 || i == servers.Length - 1)
                {
                    var sorted = latencyLog.OrderBy(x => x.Latency).ToArray();
                    File.WriteAllText("Latency-Sorted-Report.txt", string.Join(Environment.NewLine, sorted.Select(x => x.LogLine)));
                }
            }

        }


        class ServerLatency
        {
            public Server Server;
            public String LogLine;
            public double Latency;
        }
        

    }
}