using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KernelManagementJam;
using Universe.HttpWaiter;

namespace Universe.W3Top
{
    partial class Startup
    {
        private void PreJitAspNet()
        {
            var urlBase = "http://localhost:5050";
            if (IpConfig.Addresses.Any()) urlBase = IpConfig.Addresses.First();
            urlBase = urlBase.TrimEnd('/');

            string[] reqs =
            {
                $"{urlBase}/; Method=Get; Valid Status = 100-299",
                $"{urlBase}/manifest.json; Method=Get; Valid Status = 100-299",
                $"{urlBase}/api/BriefInfo; Method=Get; Valid Status = 100-299",
                $"{urlBase}/api/benchmark/disk/get-disks; Method=Get; Valid Status = 100-299",
            };

            Parallel.ForEach(reqs, (req) =>
            {
                try
                {
                    Stopwatch startAt = Stopwatch.StartNew();
                    HttpConnectionString cs = new HttpConnectionString(req);
                    HttpProbe.Go(cs).Wait();
                    Console.WriteLine($"Pre-JITed [{req}] in {startAt.Elapsed}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Warning! Optional Pre-JIT [{req}] failed. " + ex.GetExceptionDigest());
                }

            });

        }

    }
}