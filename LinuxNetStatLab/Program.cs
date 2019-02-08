using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LinuxNetStatLab
{
    class Program
    {
        static void Main(string[] args)
        {
            ReformatCopyOfNetStat();

            Stopwatch sw = Stopwatch.StartNew();

            var prev = new RawNetStatReader(new StringReader(GetRaw())).Items;
            var prevTicks = sw.ElapsedTicks;
            while (true)
            {
                Thread.Sleep(666);
                var next = new RawNetStatReader(new StringReader(GetRaw())).Items;
                var nextTicks = sw.ElapsedTicks;

                var duration = (nextTicks - prevTicks) * 1d / Stopwatch.Frequency;
                var current = new List<NetStatRow>();
                for (int i = 0; i < prev.Count && i < next.Count; i++)
                {
                    if (prev[i].Group == next[i].Group && prev[i].Key == next[i].Key)
                    {
                        current.Add(new NetStatRow() { Group = prev[i].Group, Key = prev[i].Key, Long = next[i].Long - prev[i].Long});
                    }
                }

                Console.SetCursorPosition(0,0);
                for (int i = 0; i < current.Count; i++)
                {
                    if (i > 0 && (i % 2) == 0)
                        Console.WriteLine();

                    var item = current[i];
                    var value = 1d * item.Long / duration;
                    var info = string.Format("{0,-36}: {1}", item.Group + "." + item.Key, item.Long.ToString("0.000"));
                    Console.Write(string.Format("{0,-52}  ", info));

                    prev = next;
                    prevTicks = nextTicks;
                }

            }


        }

        static string GetRaw()
        {
            var file = "/proc/net/netstat";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) file = "netstat-global";
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rdr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                return rdr.ReadToEnd();
            }
        }

        private static void ReformatCopyOfNetStat()
        {
            string[] rawNames = new[] {"netstat-global", "netstat-h3control"};
            foreach (var rawName in rawNames)
            {
                using (FileStream fs = new FileStream(rawName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader rdr = new StreamReader(fs, Encoding.ASCII))
                {
                    var items = new RawNetStatReader(rdr).Items;
                    using (FileStream fsr = new FileStream(rawName + ".report", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    using (StreamWriter wr = new StreamWriter(fsr, Encoding.ASCII))
                    {
                        foreach (var item in items)
                        {
                            wr.WriteLine(string.Format("{0,-36}: {1}", item.Group + "." + item.Key, item.Long.ToString("0")));
                        }
                    }
                }
            }
        }
    }
}
