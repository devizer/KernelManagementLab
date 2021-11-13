using System.IO;
using System.Net;
using System.Text;

namespace KernelManagementJam
{
    public class UptimeParser
    {
        static readonly Encoding Utf8 = new UTF8Encoding(false);
        
        public static double? ParseUptime()
        {
            const string path = "/proc/uptime";
            if (File.Exists(path))
            {
                var line = SmallFileReader.ReadFirstLine(path);
                string[] arr = line.Split(new char[] {' ', '\t'});
                if (arr.Length >= 2)
                {
                    if (double.TryParse(arr[0], out var ret))
                    {
                        return ret;
                    }
                }
            }

            return null;
        }
    }
}