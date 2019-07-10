using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Universe.W3Top
{
    public class KillerMiddleware
    {
        private readonly RequestDelegate _next;
        
        static Dictionary<string, long> Paths = new Dictionary<string, long>();

        public KillerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            Console.WriteLine($"Middleware.Invoke: {path}");
            // long prev = Paths.GetOrAdd(path, x => 0);
            // Paths[path] = prev + 1;
            
            await _next.Invoke(context);
        }
    }
}