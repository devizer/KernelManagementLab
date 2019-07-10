using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Universe.W3Top
{
    public class KillerActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // do something before the action executes
            // Console.WriteLine($"OnActionExecuting: {context.HttpContext.Request.Path}");
        }

        public void OnActionExecuted(ActionExecutedContext actionContext)
        {
            // Console.WriteLine($"KillerActionFilter OnActionExecuted at {actionContext.HttpContext.Request.Path}");
            var context = actionContext.HttpContext;
            if ((context.Response.ContentType ?? "").StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
            {
                // Response may already started. Readonly always true
                if (!context.Response.Headers.IsReadOnly)
                {
                    context.Response.Headers.Add("Pragma", "no-cache");
                    context.Response.Headers.Add("Cache-Control", "no-cache");
                }
            }
        }
    }
}