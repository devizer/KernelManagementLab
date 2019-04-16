using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReactGraphLab
{
    class CompressedMimeTypes
    {
        private static Lazy<string[]> _List = new Lazy<string[]>(() => Build() , LazyThreadSafetyMode.ExecutionAndPublication);
        public static IEnumerable<string> List => _List.Value;

        private static string[] Build()
        {
            string raw = @"text/plain text/x-component text/css text/xml text/javascript
   application/atom+xml application/rss+xml application/vnd.ms-fontobject
   application/x-font-ttf application/x-web-app-manifest+json application/xhtml+xml application/xml
   application/json application/javascript font/opentype image/svg+xml image/x-icon
   application/xml application/xml+rss";

            return raw
                .Split(new[] {' ', '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();
        }
    }
}