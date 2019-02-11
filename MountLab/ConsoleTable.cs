using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MountLab
{
    public class ConsoleTable
    {
        private List<List<string>> content = new List<List<string>>();
        List<string> header = new List<string>();
        List<bool> rightAlignment = new List<bool>();

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
            List<string> row = new List<string>();
            foreach (var v in values)
            {
                if (v is double?)
                {
                    double? d = (double?) v;
                    row.Add(!d.HasValue ? "-" : d.Value.ToString("f2"));
                }
                else
                {
                    row.Add(Convert.ToString(v));
                }
            }

            content.Add(row);
        }

        public override string ToString()
        {
            var copy = new List<List<string>>();
            copy.Add(header.Select(x => Convert.ToString((string) x)).ToList());
            copy.AddRange(content);
            int cols = copy.Max(x => x.Count);
            List<int> width = Enumerable.Repeat(3, cols).ToList();
            for (int y = 0; y < copy.Count; y++)
            {
                List<string> row = copy[y];
                for (int x = 0; x < row.Count; x++)
                {
                    width[x] = Math.Max(width[x], (row[x] ?? "").Length);
                }
            }
            var sep = width.Select(x => new string('-', x)).ToList();
            copy.Insert(1, sep);

            StringBuilder ret = new StringBuilder();
            for (int y = 0; y < copy.Count; y++)
            {
                List<string> row = copy[y];
                for (int x = 0; x < cols; x++)
                {
                    if (x > 0) ret.Append(y == 1 ? "+" : "|");
                    string v = (x < row.Count ? row[x] : null) ?? "";
                    if (v.Length < width[x])
                    {
                        string pad = new string(' ', -v.Length + width[x]);
                        if (rightAlignment[x] && y>0)
                            v = pad + v;
                        else
                            v = v + pad;
                    }

                    ret.Append(v);
                }
                ret.AppendLine();
            }

            return ret.ToString();
        }
    }
}