using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Universe.W3Top
{
    public class PreventSpaHtmlCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] Paths = {"/", "/index.html"};

        private static readonly Lazy<string> _Ver = new Lazy<string>(() =>
            Assembly.GetEntryAssembly().GetName().Version.ToString()
        ); 

        public PreventSpaHtmlCachingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Method 1. 
            bool isIt = Paths.Any(x => x.Equals(context.Request.Path, StringComparison.InvariantCultureIgnoreCase));
            if (isIt)
            {
                context.Response.Headers.Add("Pragma", "no-cache");
                context.Response.Headers.Add("Cache-Control", "no-cache");
            }
            
            Stopwatch startAt = Stopwatch.StartNew();

            // Method 2. 
            context.Response.OnStarting(_ => 
            {
                var msec = startAt.ElapsedTicks * 1000d / Stopwatch.Frequency;
                context.Response.Headers.Add("X-Duration-in-Milliseconds", msec.ToString("0.00"));
                context.Response.Headers.Add("X-Ver", _Ver.Value);

                var type = context.Response.ContentType ?? "";
                bool isIt2 =
                    type.StartsWith("text/html;", StringComparison.OrdinalIgnoreCase)
                    || type.Equals("text/html", StringComparison.OrdinalIgnoreCase);

                string path = context.Request.Path == null ? null : context.Request.Path.ToString(); 
                bool isIt3 = "/index.html".Equals(path, StringComparison.InvariantCultureIgnoreCase);

                if (isIt2 && isIt3)
                {
                    context.Response.Headers.Remove("ETag");
                    
                    context.Response.Headers.Remove("Pragma");
                    context.Response.Headers.Add("Pragma", "no-cache");

                    context.Response.Headers.Remove("Cache-Control");
                    context.Response.Headers.Add("Cache-Control", "no-cache");
                }
                
                return Task.CompletedTask;
            }, context);

            await _next.Invoke(context);
            
        }
    }
}
