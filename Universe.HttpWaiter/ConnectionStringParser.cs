using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Universe.HttpWaiter
{
    // Valid Status=200,403,100-499; Uri=http://mywebapi:80/get-status; Method=POST; *Accept=application/json, text/javascript; Payload={'verbosity':'normal'}"
    /*
     * Built-in System.Data.Common.DbConnectionStringBuilder has limitation:
     * All the keys are lower-cased
     * Same keys are ignored, only one wins
     * Empty key is not supported
     */
    public class ConnectionStringParser
    {
        [NotNull]
        public readonly string ConnectionString;

        [NotNull, ItemNotNull]
        public IEnumerable<Pair> Pairs => _LazyPairs.Value;

        private readonly Lazy<List<Pair>> _LazyPairs;

        public class Pair
        {

            [NotNull]
            public string Key { get; set; }

            [NotNull]
            public string Value { get; set; }

            public bool HasKey { get; set; }

            public override string ToString()
            {
                return $"{nameof(HasKey)}: {HasKey}, {nameof(Key)}: {Key}, {nameof(Value)}: {Value}";
            }
        }

        private ConnectionStringParser()
        {
            _LazyPairs = new Lazy<List<Pair>>(() => Parse_Impl());
        }

        public ConnectionStringParser(string connectionString) : this()
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        internal IEnumerable<string> GetSegments()
        {
            StringBuilder buffer = new StringBuilder();
            var arg = ConnectionString;
            var len = arg.Length;
            bool isQuote = false;
            for(int i = 0; i < len; i++)
            {
                var ch = arg[i];
                if (i < len - 1 && ch == '\\' && arg[i + 1] == '\'')
                {
                    buffer.Append('\'');
                    i++;
                }

                else if (!isQuote && ch == ';')
                {
                    yield return buffer.ToString();
                    buffer.Clear();
                }

                else if (ch == '\'')
                {
                    isQuote = !isQuote;
                }

                else
                    buffer.Append(ch);
            }

            if (buffer.Length > 0)
                yield return buffer.ToString();

        }

        List<Pair> Parse_Impl()
        {
            List<Pair> ret = new List<Pair>();
            var vars = this.
                GetSegments()
                .Select(x => x.Trim())
                .Where(x => x.Length > 0);

            foreach (var v in vars)
            {
                int p = v.IndexOf('=');
                if (p < 0)
                {
                    ret.Add(new Pair { Key = v, Value = v, HasKey = false });
                }
                else
                {
                    string key = p > 0 ? v.Substring(0, p).Trim() : "";
                    string value = p<v.Length-1 ? v.Substring(p + 1).Trim() : "";
                    ret.Add(new Pair
                    {
                        Key = key,
                        Value = value,
                        HasKey = !string.IsNullOrEmpty(key)
                    });
                }
            }

            return ret;
        }

        public string AsHumanReadable(int intent)
        {
            Func<Pair, string> keyDescription = pair => pair.HasKey ? $"[{pair.Key}]" : "Main";
            string pre = intent > 0 ? new string(' ', intent) : "";
            return string.Join(Environment.NewLine, Pairs.Select(x =>
                    string.Format("{0}{1}: {2}", pre, keyDescription(x), x.Value)
            ));
        }

    }

    public static class ConnectionStringParserExtensions
    {
        const StringComparison Ignore = StringComparison.OrdinalIgnoreCase;

        public static bool GetFirstBool(this IEnumerable<ConnectionStringParser.Pair> pairs, string parameterName, bool defVal = false)
        {
            return pairs.FirstOrDefault(x => parameterName.Equals(x.Key, Ignore))?.IsItTrue() ?? defVal;
        }

        public static int GetFirstInt(this IEnumerable<ConnectionStringParser.Pair> pairs, string parameterName, int defVal, int min = 0, int max = Int32.MaxValue)
        {
            var raw = pairs.FirstOrDefault(x => parameterName.Equals(x.Key, Ignore))?.Value;
            int ret;
            if (raw == null || !int.TryParse(raw, out ret))
                ret = defVal;

            return Math.Max(Math.Min(ret, max), min);
        }

        public static string GetFirstString(this IEnumerable<ConnectionStringParser.Pair> pairs, string parameterName)
        {
            return pairs.FirstOrDefault(x => parameterName.Equals(x.Key, Ignore))?.Value;
        }

        public static string GetFirstWithoutKey(this IEnumerable<ConnectionStringParser.Pair> pairs)
        {
            return pairs.FirstOrDefault(x => !x.HasKey && !string.IsNullOrWhiteSpace(x.Value))?.Value;
        }

        public static bool IsItTrue(this ConnectionStringParser.Pair pair)
        {
            var v = pair.Value;
            return v != null &&
                   ("True".Equals(v, Ignore)
                    || "On".Equals(v, Ignore)
                    || "Yes".Equals(v, Ignore));

        }

    }

}
