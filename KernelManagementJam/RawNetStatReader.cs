using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KernelManagementJam
{
    public class RawNetStatReader
    {
        public readonly TextReader Reader;

        public RawNetStatReader(TextReader reader)
        {
            Reader = reader;
            Items = new List<NetStatRow>();
            Read();
        }

        public List<NetStatRow> Items { get; }

        private void Read()
        {
            var lines = new List<string>();
            string line = null;
            do
            {
                line = Reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            } while (line != null);

            for (var i = 0; i < lines.Count - 1; i += 2)
            {
                var arr1 = lines[i].Split(':');
                var arr2 = lines[i + 1].Split(':');
                if (arr1.Length == arr2.Length && arr1.Length == 2 && arr1[0] == arr2[0])
                {
                    var group = arr1[0];
                    var keys = arr1[1].Split(' ').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
                    var values = arr2[1].Split(' ').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
                    if (keys.Length != values.Length)
                    {
                        Console.WriteLine("DIFFERENT LENGTH");
                        Debugger.Break();
                    }

                    for (var k = 0; k < keys.Length; k++) Items.Add(new NetStatRow {Group = group, Key = keys[k], Long = long.Parse(values[k])});
                }
            }
        }
    }

    public class NetStatReport
    {
        public double Duration { get; set; }
        public IList<NetStatRow> Rows { get; set; }
    }

    public class NetStatRow
    {
        public string Group { get; set; }
        public string Key { get; set; }
        public long Long { get; set; }
    }
}