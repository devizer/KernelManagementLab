using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Universe.FioStream
{
    public class FioStreamReader
    {

        public void ReadStreamToEnd(StreamReader streamReader)
        {
            while(true)
            {
                string? line = streamReader.ReadLine();
                if (line != null)
                    ReadNextLine(line);
                else 
                    return;
            }
        }
        
        public void ReadNextLine(string line)
        {
            var lineLength = line.Length;
            var colonCharIndex = line.IndexOf(':');
            if (colonCharIndex == 0 || colonCharIndex == lineLength - 1) return;
            // Progress - Jobs: ...
            // summary:
            //    read: IOPS=24.5k, BW=95.7MiB/s (100MB/s)(287MiB/3002msec) /* 3.6 */
            //    read : io=4288.0MB, bw=1416.6MB/s, iops=1416 , runt=  3027msec /* 2.0 */
            //    write: IOPS=22.9k, BW=89.4MiB/s (93.8MB/s)(269MiB/3003msec); 0 zone resets /* 3.26 */
            //    read: IOPS=136, BW=136MiB/s (143MB/s)(448MiB/3288msec)", /* 2.21 */
            var key1 = line.Substring(0, colonCharIndex);
            var value1 = line.Substring(colonCharIndex + 1);
            key1 = key1.Trim();
            if (key1.Equals("Jobs", StringComparison.InvariantCultureIgnoreCase))
            {
                var brakets = ReadBracketSections(value1).ToArray();
                Console.WriteLine($"{brakets.Length} Brakets: {string.Join(", ",brakets)}");
            }
        }

        static IEnumerable<string> ReadBracketSections(string raw)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in raw)
            {
                if (c == '[') sb.Clear();
                else if (c == ']')
                {
                    yield return sb.ToString();
                }
                else
                    sb.Append(c);
            }
        }
    }
}