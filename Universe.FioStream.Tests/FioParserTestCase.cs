using System;
using System.Collections.Generic;
using System.Linq;

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