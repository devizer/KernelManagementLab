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
            NetStatItems = new List<NetStatRow>();
            Read();
        }

        public List<NetStatRow> NetStatItems { get; }

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
                var arr1Names = lines[i].Split(':');
                var arr2Values = lines[i + 1].Split(':');
                if (arr1Names.Length == arr2Values.Length && arr1Names.Length == 2 && arr1Names[0] == arr2Values[0])
                {
                    var group = arr1Names[0];
                    var rawKeys = arr1Names[1].Split(' ');
                    var keys = rawKeys.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
                    var rawValues = arr2Values[1].Split(' ');
                    var values = rawValues.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
                    if (keys.Length != values.Length)
                    {
                        var message = $"/proc/net/netstat is corrupted. " +
                                      $"Number of keys [{keys.Length}] differs from number of values [{values.Length}]. " +
                                      $"Keys: [{rawKeys}]. " +
                                      $"Values: [{rawValues}]";
                        
                        throw new InvalidOperationException(message);
                    }

                    for (var k = 0; k < keys.Length; k++)
                    {
                        if (!long.TryParse(values[k], out var longValue))
                            throw new InvalidOperationException(
                                $"/proc/net/netstat is corrupted. " +
                                $"Value of {group}.{keys[k]} (position is {k}) should be a long value, but it is the [{values[k]}]");
                        
                        NetStatItems.Add(new NetStatRow
                        {
                            Group = group,
                            Key = keys[k],
                            Long = longValue
                        });
                    }
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