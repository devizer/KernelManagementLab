using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoGeneratedTests
{
    public partial class FioParserTestCase {
        public override string ToString()
        {
            return $"fio-{Version}";
        }

        static FioParserTestCase()
        {
            var sorted = All.OrderBy(x => new Version(x.Version));
            All = sorted.ToArray();
        }
    }

    public class FioParserTestCase2
    {
        public string Version;
        public string Method;
        public string[] Lines;

        public override string ToString()
        {
            return $"{Version}: {Method}, {Lines.Length} lines";
        }

        public static IEnumerable<FioParserTestCase2> GetAll_V3()
        {
            var methods = new[]
            {
                new {Name = "Seq Read", File = "read.log"},
                new {Name = "Seq Write", File = "write.log"},
                new {Name = "Rand Read", File = "randread.log"},
                new {Name = "Rand Write", File = "randwrite.log"},
            };

            var dirs = new DirectoryInfo("fio-test-cases").GetDirectories();
            foreach (DirectoryInfo dir in dirs)
            {
                var version = dir.Name;
                foreach (var method in methods)
                {
                    var file = Path.Combine(dir.FullName, method.File);
                    var lines = ReadLines(file);
                    yield return new FioParserTestCase2()
                    {
                        Lines = lines,
                        Method = method.Name,
                        Version = TryParseVersion(version),
                    };
                }
            }
        }

        static string TryParseVersion(string raw)
        {
            if (raw.Length >= 5)
            {
                if (raw.StartsWith("fio-", StringComparison.OrdinalIgnoreCase))
                    raw = raw.Substring(4);
                if (raw.StartsWith("fio ", StringComparison.OrdinalIgnoreCase))
                    raw = raw.Substring(4);
            }

            return raw;
        }

        static string[] ReadLines(string fileName)
        {
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rdr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                var all = rdr.ReadToEnd();
                return all.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static IEnumerable<FioParserTestCase2> GetAll()
        {
            foreach (var testCase in FioParserTestCase.All)
            {
                var methods = new[]
                {
                    new {Name = "Seq Read", Lines = testCase.SeqRead},
                    new {Name = "Seq Write", Lines = testCase.SeqWrite},
                    new {Name = "Rand Read", Lines = testCase.RandRead},
                    new {Name = "Rand Write", Lines = testCase.RandWrite},
                };
                foreach (var method in methods)
                {
                    yield return new FioParserTestCase2()
                    {
                        Version = testCase.Version,
                        Method = method.Name,
                        Lines = method.Lines,
                    };
                }

            }
        } 
    }
}