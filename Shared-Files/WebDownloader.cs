using System;
using System.IO;
using System.Net;
#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_3
using System.Net.Http;
#endif
using System.Threading.Tasks;

namespace Universe.Shared
{
    public class WebDownloader
    {
        public byte[] DownloadContent(string url)
        {
            ConfigureCertificateValidation();

#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_3
            return DownloadContentAsync(url).Result;
#else
            using (var wc = new System.Net.WebClient())
            {
                wc.Headers["User-Agent"] = "w3-fio";
                wc.Proxy = System.Net.WebRequest.DefaultWebProxy;
                return wc.DownloadData(new Uri(url));
            }
#endif
        }

        public void DownloadFile(string url, string toFile)
        {
            ConfigureCertificateValidation();

#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_3
            DownloadFileAsync(url, toFile).Wait();
#else
            
            using (var wc = new System.Net.WebClient())
            {
                wc.Headers["User-Agent"] = "w3-fio";
                wc.Proxy = System.Net.WebRequest.DefaultWebProxy;
                wc.DownloadFile(new Uri(url), toFile);
            }
#endif
        }

#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_3

        private async Task<byte[]> DownloadContentAsync(string url)
        {
            using (var client = new System.Net.Http.HttpClient(_ClientHandler.Value))
            {
                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        using (MemoryStream mem = new MemoryStream())
                        {
                            Stream stream = await result.Content.ReadAsStreamAsync();
                            await stream.CopyToAsync(mem);
                            return mem.ToArray();
                        }
                    }
                    else
                    {
                        throw new Exception($"{url} is not accessible. Status: {result.StatusCode}");
                    }
                }
            }
        }  
        
        private async Task DownloadFileAsync(string url, string toFile)
        {
            using (var client = new System.Net.Http.HttpClient(_ClientHandler.Value))
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
        
        private static Lazy<HttpClientHandler> _ClientHandler = new Lazy<HttpClientHandler>(() =>
            {
                var handler = new HttpClientHandler();
                handler.AllowAutoRedirect = true;
                handler.ServerCertificateCustomValidationCallback += (requestMessage, x509Certificate2, x509Chain, sslPolicyErrors) => true;
                return handler;
            }
        );

#endif

        private static bool IsCertificateValidationConfigured = false;
        static readonly object SyncCertificateValidation = new object();

        static void ConfigureCertificateValidation()
        {
            if (IsCertificateValidationConfigured) return;
            lock (SyncCertificateValidation)
            {
                if (IsCertificateValidationConfigured) return;
#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_3
                // nothing can do here
#else
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
#endif
                IsCertificateValidationConfigured = true;
            }
        }
    }
}
