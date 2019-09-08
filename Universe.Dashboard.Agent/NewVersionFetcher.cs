using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using KernelManagementJam;
using Newtonsoft.Json.Linq;

namespace Universe.Dashboard.Agent
{
    public class NewVersionFetcher
    {
        public static readonly string Url = "https://raw.githubusercontent.com/devizer/w3top-bin/master/public/version.json";
        
        static ManualResetEvent FirstRoundDone = new ManualResetEvent(false);

        // Startup|test must fail if NewVersionDataSource.InitialValue is malformed
        public static void Configure()
        {
            var tryFail = NewVersionDataSource.NewVersion;
            int waitDurationOnFail = 1;
            Thread t = new Thread(() =>
            {
                while (!PreciseTimer.Shutdown.WaitOne(0))
                {
                    bool isOk = Iteration();
                    FirstRoundDone.Set();
                    waitDurationOnFail = isOk ? 1 : Math.Min(60, waitDurationOnFail * 2);
                    var sleepDuration = isOk ? 5*60 : waitDurationOnFail;
                    PreciseTimer.Shutdown.WaitOne(sleepDuration * 1000);
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
                    handler.ServerCertificateCustomValidationCallback += (message, certificate, chain, error) =>
                    {
                        // ca-certificates package may be too old or malformed 
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
                Console.WriteLine($"INFO: Info about new version was not fetched via {Url}. {ex.GetExceptionDigest()}");
                return false;
            }
        }
    }
}