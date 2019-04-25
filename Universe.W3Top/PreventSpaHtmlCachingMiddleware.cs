using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;

namespace ReactGraphLab
{
    public class PreventSpaHtmlCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] Paths = {"/", "/index.html"};

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

                Console.WriteLine($"Path: [{context.Request.Path}], PathBase: {context.Request.PathBase}");
                var type = context.Response.ContentType ?? "";
                bool isIt2 =
                    type.StartsWith("text/html;", StringComparison.OrdinalIgnoreCase)
                    || type.Equals("text/html", StringComparison.OrdinalIgnoreCase);

                if (isIt2)
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
