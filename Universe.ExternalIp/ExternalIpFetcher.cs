using System.Text;
using Universe.Shared;

namespace Universe.ExternalIp
{
    public class ExternalIpFetcher
    {
        private static Encoding Utf8 = new UTF8Encoding(false);
        public static string Fetch()
        {
            WebDownloader wd = new WebDownloader();
            var bytes = wd.DownloadContent("https://api.ipify.org");
            var rawIp = Utf8.GetString(bytes);
            return rawIp; 
        }
    }
}
