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
                $"{urlBase}/; Method=Get; Valid Status = 100-399",
                $"{urlBase}/manifest.json; Method=Get; Valid Status = 100-399",
                $"{urlBase}/api/BriefInfo; Method=Get; Valid Status = 100-399",
                $"{urlBase}/api/benchmark/disk/get-disks; Method=Get; Valid Status = 100-399",
                $"{urlBase}/api/benchmark/disk/get-disk-benchmark-history; Method=Post; Valid Status = 100-399",
                $"{urlBase}/api/benchmark/disk/get-disk-benchmark-progress-{Guid.NewGuid()}; Method=Post; Valid Status = 100-399",
            };

            ParallelOptions po = new ParallelOptions() {MaxDegreeOfParallelism = 2};
            Parallel.ForEach(reqs, po, (req) =>
            {
                Stopwatch swFail = Stopwatch.StartNew();
                try
                {
                    double[] durations = new double[2]; 
                    for (int i = 0; i <= 1; i++)
                    {
                        Stopwatch startAt = Stopwatch.StartNew();
                        HttpConnectionString cs = new HttpConnectionString(req);
                        HttpProbe.Go(cs).Wait();
                        durations[i] = startAt.ElapsedTicks * 1000d / Stopwatch.Frequency;
                    }
                    Console.WriteLine($"Pre-JITed [{req}]. First: {durations[0]:n1}, Next: {durations[1]:n1} msecs");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Warning! Optional Pre-JIT [{req}] failed ({swFail.ElapsedMilliseconds:n1} msec). " + ex.GetExceptionDigest());
                }

            });

        }

    }
}