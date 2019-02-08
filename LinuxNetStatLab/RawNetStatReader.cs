using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinuxNetStatLab
{
    class RawNetStatReader
    {
        public readonly TextReader Reader;

        public List<NetStatRow> Items { get; private set; } 

        public RawNetStatReader(TextReader reader)
        {
            Reader = reader;
            Items = new List<NetStatRow>();
            Read();
        }

        private void Read()
        {
            List<string> lines = new List<string>();
            string line = null;
            do
            {
                line = Reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            } while (line != null);

            for (int i = 0; i < lines.Count - 1; i += 2)
            {
                var arr1 = lines[i].Split(':');
                var arr2 = lines[i+1].Split(':');
                if (arr1.Length == arr2.Length && arr1.Length == 2 && arr1[0] == arr2[0])
                {
                    string group = arr1[0];
                    var keys = arr1[1].Split(' ').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
                    var values = arr2[1].Split(' ').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
                    if (keys.Length != values.Length)
                    {
                        Console.WriteLine("DIFFERENT LENGTH");
                        Debugger.Break();
                    }

                    for(int k=0; k<keys.Length; k++)
                    {
                        Items.Add(new NetStatRow() { Group = group, Key = keys[k], Long = long.Parse(values[k])});
                        
                    }
                }
            }
        }
    }

    public class NetStatReport
    {
        public double Duration { get; set; }
        public IReadOnlyList<NetStatRow> Rows { get; set; }
    }

    public class NetStatRow
    {
        public string Group { get; set; }
        public string Key { get; set; }
        public long Long { get; set; }
    }

}
