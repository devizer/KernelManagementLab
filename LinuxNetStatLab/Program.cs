using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KernelManagementJam;

namespace LinuxNetStatLab
{
    class Program
    {
        private static int PID = 0;

        private static int LabelWidth = 36;

        static void Main(string[] args)
        {
            var splitted = SpaceSeparatedDecoder.DecodeIntoColumns(@"tmpf /with\040space /fuck\134off /fuck-по-русски-off /fuck\\off");
            Console.WriteLine(string.Join(" ", splitted));

            if (args.Length >= 1) int.TryParse(args[0], out PID);
            
            ReformatCopyOfNetStat();

            Stopwatch sw = Stopwatch.StartNew();

            var prev = new RawNetStatReader(new StringReader(GetRaw())).Items;
            var prevTicks = sw.ElapsedTicks;
            Console.Clear();
            while (true)
            {
                Thread.Sleep(1);
                var totalWidth = Console.WindowWidth;
                var totalColumns = Math.Max(1, totalWidth / (LabelWidth + 18));
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

                var pidInfo = string.Format("{0}", PID == 0 ? "ANY" : PID.ToString("0"));
                if (PID != 0) pidInfo += " " + Process.GetProcessById(PID).ProcessName;
                StringBuilder report = new StringBuilder();
                report.AppendLine(string.Format("PID: {0}", pidInfo));
                for (int i = 0; i < current.Count; i++)
                {
                    if (i > 0 && (i % totalColumns) == 0)
                        report.AppendLine();

                    var item = current[i];
                    var value = item.Long == 0 ? "" : (item.Long / duration).ToString("n0");
                    var label = item.Group + "." + item.Key;
                    if (label.Length < LabelWidth) label += new string(' ', LabelWidth - label.Length);
                    else if (label.Length > LabelWidth) label = label.Substring(0, LabelWidth);
                    var info = string.Format("{0}: {1,14}  ", label, value);
                    report.AppendFormat(info);
                }

                prev = next;
                prevTicks = nextTicks;

                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.Write(report);

                var userKey = GetConsoleKey(666);
                if (userKey?.Key == ConsoleKey.RightArrow)
                    LabelWidth = Math.Min(60, LabelWidth + 1);
                else if (userKey?.Key == ConsoleKey.LeftArrow)
                    LabelWidth = Math.Max(4, LabelWidth - 1);

                if (userKey != null)
                {
                    Console.Write($"  '{userKey.Value.KeyChar}', {userKey.Value.Key}");
                    // Thread.Sleep(333);
                }


            }


        }

        static ConsoleKeyInfo? GetConsoleKey(long milliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            do
            {
                if (Console.KeyAvailable)
                    return Console.ReadKey();

                Thread.Sleep(1);

            } while (sw.ElapsedMilliseconds <= milliseconds);

            return null;
        }

        static string GetRaw()
        {
            var file = PID > 0 ? string.Format("/proc/{0}/net/netstat", PID) : "/proc/net/netstat";
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
