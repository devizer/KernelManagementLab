using System;
using System.Collections.Generic;
using System.Linq;

namespace Universe.HttpWaiter
{
    // Valid Status=200,403,100-499; Uri=http://mywebapi:80/status; Method=POST; Accept=application/json, text/javascript; Payload={'verbosity':'normal'}"
    public class HttpConnectionString : ConnectionStringParser
    {
        private Lazy<string> _Uri;
        private Lazy<ExpectedStatusCodes> _ExpectedStatus;
        private Lazy<string> _Method;
        private Lazy<int> _Timeout;
        private Lazy<IEnumerable<Header>> _Headers;
        private Lazy<string> _Payload;

        public string Uri => _Uri.Value;

        public ExpectedStatusCodes ExpectedStatus => _ExpectedStatus.Value;

        public int Timeout => _Timeout.Value;

        public IEnumerable<Header> Headers => _Headers.Value;

        public string Payload => _Payload.Value;

        public string Method => _Method.Value;

        public HttpConnectionString(string connectionString) : base(connectionString)
        {
            _Uri = new Lazy<string>(() =>
            {
                var ret = Pairs.GetFirstString("Uri") ?? Pairs.GetFirstWithoutKey();
                if (string.IsNullOrEmpty(ret)) throw new ArgumentException("Uri parameter is expected");
                return ret;
            });

            _Timeout = new Lazy<int>(() =>
            {
                return Pairs.GetFirstInt("Timeout", defVal: 30000, min: 1);
            });

            _Method = new Lazy<string>(() =>
            {
                return Pairs.GetFirstString("Method") ?? "Get";
            });

            _Payload = new Lazy<string>(() =>
            {
                return Pairs.GetFirstString("Payload");
            });

            _ExpectedStatus = new Lazy<ExpectedStatusCodes>(() =>
            {
                string v = Pairs.GetFirstString("Valid Status");
                return v == null ? ExpectedStatusCodes.Any : new ExpectedStatusCodes(v);
            });

            _Headers = new Lazy<IEnumerable<Header>>(() =>
            {

                var lookup = Pairs
                    .Where(x => x.HasKey && x.Key.TrimStart().StartsWith("*"))
                    .ToLookup(x => x.Key.TrimStart(), x => x.Value);

                List<Header> ret = new List<Header>();
                foreach (var item in lookup)
                {
                    ret.Add(new Header()
                    {
                        Name = item.Key.Substring(1),
                        Values = new List<string>(item.ToList())
                    });
                }

                // if (Debugger.IsAttached && ConnectionString.IndexOf("Smart") >= 0) Debugger.Break();
                return ret;
            });
        }

        public class ExpectedStatusCodes
        {
            private string _Raw = "100-403,405-499";
            private List<int[]> Parsed;

            public static readonly ExpectedStatusCodes Any = new ExpectedStatusCodes("1-999999999");

            // TODO: .ctor should not throw exceptions
            public ExpectedStatusCodes(string validCodes)
            {
                if (validCodes == null) throw new ArgumentNullException("validCodes");

                _Raw = validCodes;
                Parsed = new List<int[]>();
                var arr = _Raw.Split(',', '|');
                foreach (var item in arr)
                {
                    int[] ints;
                    try
                    {
                        ints = item.Split('-').Select(x => int.Parse(x.Trim())).ToArray();
                    }
                    catch (FormatException ex)
                    {
                        throw new InvalidOperationException($"Valid status code '{_Raw}' is invalid", ex);
                    }

                    Parsed.Add(ints);
                }

            }

            public bool IsValid(int status)
            {

                foreach (var ints in Parsed)
                {
                    var intsLength = ints.Length;

                    if (intsLength == 1 && ints[0] == status)
                        return true;

                    else if (intsLength == 2
                        && ints[0] <= status 
                        && ints[1] >= status)
                        return true;
                }

                return false;
            }
        }

        public class Header
        {
            public string Name { get; set; }
            public IEnumerable<string> Values { get; set; }

            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, {nameof(Values)}: [{string.Join(",", Values)}]";
            }
        }


    }
}
