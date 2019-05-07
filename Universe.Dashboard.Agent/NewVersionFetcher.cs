using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Universe.Dashboard.Agent
{
    public class NewVersionFetcher
    {
        public static readonly string Url =
            "https://raw.githubusercontent.com/devizer/w3top-bin/master/public/version.json";
        
        static ManualResetEvent FirstRoundDone = new ManualResetEvent(false);

        // Startup|test must fail if InitialValue is malformed
        public static void Configure()
        {
            var tryFail = NewVersionDataSource.NewVersion;
            Thread t = new Thread(() =>
            {
                while (!PreciseTimer.Shutdown.WaitOne(0))
                {
                    bool isOk = Iteration();
                    FirstRoundDone.Set();
                    var sleepDuration = isOk ? 5*60*1000 : 1000;
                    PreciseTimer.Shutdown.WaitOne(sleepDuration);
                }
            }) { IsBackground = true, Name = "New Version Fetcher"};
            
            t.Start();
        }

        static bool Iteration()
        {
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    handler.AllowAutoRedirect = true;
                    handler.ServerCertificateCustomValidationCallback += (message, certificate2, chain, error) =>
                    {
                        return true;
                    };

                    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, Url);
                    var response = httpClient.SendAsync(req).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var jsonString = response.Content.ReadAsStringAsync().Result;
                        var newVer = JObject.Parse(jsonString);
                        // Console.WriteLine($"New Ver: {newVer["Version"]}");
                        NewVersionDataSource.NewVersion = newVer;
                        
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: New version info was not fetched via {Url}. {ex.GetExceptionDigest()}");
                return false;
            }
        }
    }
}