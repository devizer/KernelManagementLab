using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Universe.Dashboard.DAL.Tests.MultiProvider
{
    public class MultiProviderExtensions
    {
        public static List<string> GetServerConnectionStrings(string pattern)
        {
            var query = Environment.GetEnvironmentVariables().OfType<DictionaryEntry>()
                .Select(x => new {name = Convert.ToString(x.Key), cs = Convert.ToString(x.Value).Trim()})
                .OrderBy(x => x.name)
                .Where(x => x.name.StartsWith(pattern, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => !string.IsNullOrEmpty(x.cs))
                .OrderBy(x => x.cs);

            return query.Select(x => x.cs).ToList();
        }
    }
}