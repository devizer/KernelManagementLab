using System;
using System.Collections.Generic;
using System.Linq;

namespace Universe.W3Top
{
    class IpConfig
    {
        public static readonly List<string> Addresses = new List<string>();

        public static void AddAddresses(IEnumerable<string> addresses)
        {
            Func<string, int> orderBy = x => x.StartsWith("http://") ? 0 : 1;
            Addresses.AddRange(addresses.Select(Translate).OrderBy(orderBy));
        }

        static string Translate(string x)
        {
            return x
                .Replace("://+", "://localhost")
                .Replace("://0.0.0.0", "://localhost")
                .Replace("://[::]", "://localhost");
        }

    }
}