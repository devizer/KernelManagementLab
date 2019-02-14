using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KernelManagementJam
{
    public class ConsoleTable
    {
        private readonly List<List<string>> content = new List<List<string>>();
        private readonly List<string> header = new List<string>();
        private readonly List<bool> rightAlignment = new List<bool>();

        public ConsoleTable(params string[] columns)
        {
            foreach (var column in columns)
            {
                rightAlignment.Add(column.StartsWith("-"));
                header.Add(column.TrimStart('-'));
            }
        }

        public void AddRow(params object[] values)
        {
            var row = new List<string>();
            foreach (var v in values)
                if (v is double?)
                {
                    var d = (double?) v;
                    row.Add(!d.HasValue ? "-" : d.Value.ToString("f2"));
                }
                else
                {
                    row.Add(Convert.ToString(v));
                }

            content.Add(row);
        }

        public override string ToString()
        {
            var copy = new List<List<string>>();
            copy.Add(header.Select(x => Convert.ToString(x)).ToList());
            copy.AddRange(content);
            var cols = copy.Max(x => x.Count);
            var width = Enumerable.Repeat(1, cols).ToList();
            for (var y = 0; y < copy.Count; y++)
            {
                var row = copy[y];
                for (var x = 0; x < row.Count; x++) width[x] = Math.Max(width[x], (row[x] ?? "").Length);
            }

            var sep = width.Select(x => new string('-', x)).ToList();
            copy.Insert(1, sep);

            var ret = new StringBuilder();
            for (var y = 0; y < copy.Count; y++)
            {
                var row = copy[y];
                for (var x = 0; x < cols; x++)
                {
                    if (x > 0) ret.Append(y == 1 ? "+" : "|");
                    var v = (x < row.Count ? row[x] : null) ?? "";
                    if (v.Length < width[x])
                    {
                        var pad = new string(' ', -v.Length + width[x]);
                        if (rightAlignment[x] && y > 0)
                            v = pad + v;
                        else
                            v = v + pad;
                    }

                    if (x == cols - 1) v = (v ?? "").TrimEnd();
                    ret.Append(v);
                }

                ret.AppendLine();
            }

            return ret.ToString();
        }
    }
}