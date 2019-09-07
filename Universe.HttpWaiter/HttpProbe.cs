using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Universe.HttpWaiter
{
    public class HttpProbe
    {
        public static async Task Go(HttpConnectionString cs, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken == default(CancellationToken))
                cancellationToken = CancellationToken.None;

            HttpClient c = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(cs.Timeout),
            };

            HttpRequestMessage req = new HttpRequestMessage(
                new HttpMethod(cs.Method.ToUpper()), 
                new Uri(cs.Uri)
                );

            if (cs.Payload != null)
                req.Content = new StringContent(cs.Payload, Encoding.UTF8);

            // if (cs.ConnectionString.IndexOf("Smart") >= 0 && Debugger.IsAttached) Debugger.Break();

            var copy = new List<HttpConnectionString.Header>(cs.Headers.ToList());
            var contentType = copy.FirstOrDefault(x => "Content-Type".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (contentType != null)
            {
                req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType.Values.First());
                copy.Remove(contentType);
            }

            foreach (var header in copy)
            {
                bool isOk = false;
                Exception exception = null;
                try
                {
                    req.Content.Headers.Add(header.Name, header.Values);
                    isOk = true;

                }
                catch (Exception ex)
                {
                    try
                    {
                        exception = ex;
                        req.Headers.Add(header.Name, header.Values);
                        isOk = true;
                    }
                    catch (Exception ex2)
                    {
                        exception = ex2;
                    }
                }

                if (!isOk)
                {
                    throw new InvalidOperationException(
                        $"Unable to add header '{header.Name}'", exception);
                }

            }

            // if (cs.ConnectionString.IndexOf("Smart") >= 0 && Debugger.IsAttached) Debugger.Break();

            var response = await c.SendAsync(req, cancellationToken);
            var statusCode = response.StatusCode;
            int statusInt = (int) statusCode;
            bool isValid = cs.ExpectedStatus.IsValid(statusInt);
            if (!isValid)
                throw new InvalidOperationException($"Returned status code {statusInt} does not conform expected value. Request: \"{cs.ConnectionString}\"");


        }

        private static readonly Dictionary<string, HttpMethod> MethodsByString = new Dictionary<string, HttpMethod>(StringComparer.OrdinalIgnoreCase)
        {
            {"Get", HttpMethod.Get},
            {"Put", HttpMethod.Put},
            {"Delete", HttpMethod.Delete},
            {"Head", HttpMethod.Head},
            {"Options", HttpMethod.Options},
            {"Post", HttpMethod.Post},
            {"Trace", HttpMethod.Trace},
        };

    }
}
