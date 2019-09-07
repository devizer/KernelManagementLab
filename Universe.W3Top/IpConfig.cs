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
            Func<string, string> trans = x => x
                .Replace("://+", "://localhost")
                .Replace("://0.0.0.0", "://localhost")
                .Replace("://[::]", "://localhost");
            
            Addresses.AddRange(addresses.Select(trans));
            SortBySecurity();
        }

        /* public */ static void SortBySecurity()
        {
            var copy = Addresses.OrderBy(x => x.StartsWith("http://") ? 0 : 1).ToArray();
            Addresses.Clear();
            Addresses.AddRange(copy);
        }
    }
}