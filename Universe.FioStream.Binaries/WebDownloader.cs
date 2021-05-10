using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Universe.FioStream.Binaries
{
    public class WebDownloader
    {
        public void Download(string url, string toFile)
        {
#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_3
            Download2(url, toFile).Wait();
#else
            var wc = new System.Net.WebClient();
            wc.Proxy = WebRequest.DefaultWebProxy;
            wc.DownloadFile( new Uri(url), toFile);
#endif
        }

#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_3
        private async Task Download2(string url, string toFile)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        using (FileStream fs = new FileStream(toFile, FileMode.Create, FileAccess.Write,
                            FileShare.ReadWrite))
                        {
                            await result.Content.CopyToAsync(fs);
                        }
                    }
                    else
                    {
                        throw new Exception($"{url} is not accessible. Status: {result.StatusCode}");
                    }
                }
            }
        }
#endif
    }
}