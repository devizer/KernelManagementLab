using System;
using System.Linq;

namespace VisualizeFioTestMatrix
{
    static class FioHeaderFormatter
    {
        class Pair
        {
            public string Key, Value;

            public override string ToString()
            {
                return $"{Key}:{Value}";
            }
        }
        
        public static string Format(string raw)
        {
            // ver-3
            if (raw.StartsWith("fio="))
                return FormatVer3(raw);
            
            if (raw.StartsWith("fio-"))
                return FormatVer2(raw);
                
            return raw;
        }

        private static string FormatVer2(string raw)
        {
            var list = raw.Split('-').Skip(1).ToList();
            list.RemoveAt(3);

            var ver = list[0];
            var os = list[3];

            bool hasLibAio = list[2] != "missing";

            return ver + ".v2" + Environment.NewLine + os + Environment.NewLine +
                   (hasLibAio ? "libaio" : "posixaio");



            // return string.Join(" ", list);



        }

        private static string FormatVer3(string raw)
        {
            raw = raw.Replace("glibc=", "libc=").Replace("gcc=", "cc=");

            var arr = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            Pair ToPair(string x)
            {
                var a1 = x.Split('=');
                return new Pair() { Key = a1[0], Value = a1[1] };
            }

            var dic = arr.Select(ToPair).ToList();

            void RemoveKey(string key)
            {
                var found = dic.FirstOrDefault(x => x.Key == key);
                if (found != null) dic.Remove(found);
            }

            RemoveKey("cpu");
            RemoveKey("shared");
            RemoveKey("mode");
            var os = dic.FirstOrDefault(x => x.Key == "os");
            if (os != null)
            {
                var arr2 = os.Value.Split('_');
                os.Value = string.Join(":", arr2.Take(2));
            }

            RemoveKey("os");

            var cc = dic.FirstOrDefault(x => x.Key == "cc");
            cc.Value = new Version(cc.Value).ToString(2);

            var fio = dic.FirstOrDefault(x => x.Key == "fio")?.Value;
            RemoveKey("fio");

            return fio + ".v3" + Environment.NewLine + (os?.Value ?? "none") + Environment.NewLine + string.Join(" ", dic.Select(x => x));
        }
    }
}
