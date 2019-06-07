using System;
using System.Collections.Generic;
using System.Linq;
using KernelManagementJam;
using KernelManagementJam.Benchmarks;
using KernelManagementJam.DebugUtils;
using Microsoft.AspNetCore.Mvc;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.Agent;
using Universe.Dashboard.DAL;
using Universe.DiskBench;

namespace ReactGraphLab.Controllers
{
    [Route("api/benchmark/disk")]
    [ApiController]
    public class DiskBenchmarkController : ControllerBase 
    {
        private DiskBenchmarkQueue Queue;
        private DashboardContext Db;

        public DiskBenchmarkController(DiskBenchmarkQueue queue, DashboardContext db)
        {
            Queue = queue;
            Db = db;
        }

        [HttpGet, Route("get-disks")]
        public List<DriveDetails> GetList()
        {
            return MountsDataSource.Mounts.FilterForHuman().OrderBy(x => x.MountEntry.MountPath).ToList();
        }

        [HttpPost, Route("start")]
        public BenchmarkResponse StartBenchmark(StartBenchmarkArgs options)
        {
            // return new BenchmarkResponse() {Token = Guid.NewGuid()};
            Console.WriteLine($"StartBenchmark options: {options.AsJson()}");

            DiskBenchmark diskBenchmark = new DiskBenchmark(
                options.MountPath,
                fileSize: options.WorkingSet * 1024L * 1024L,
                flavour: DataGeneratorFlavour.ILCode,
                randomAccessBlockSize:options.BlockSize,
                stepDuration:options.RandomAccessDuration * 1000,
                disableODirect:options.DisableODirect && false,
                threadsNumber: options.Threads
            );
            
            Guid token = Guid.NewGuid();
            Queue.Enqueue(token, diskBenchmark);
            return new BenchmarkResponse()
            {
                Token = token,
                Progress = diskBenchmark.Prorgess.Clone(),
            };
        }

        [HttpPost, Route("get-disk-progress-{benchmarkToken}")]
        public BenchmarkResponse GetProgress(Guid benchmarkToken)
        {
            var benchmark = Queue.Find(benchmarkToken);
            if (benchmark == null)
            {
                var progress = Db.DiskBenchmark.FirstOrDefault(x => x.Token == benchmarkToken.ToString())?.Report;
                return new BenchmarkResponse()
                {
                    Token = benchmarkToken,
                    Progress = progress,
                };
            }
            
            return new BenchmarkResponse()
            {
                Token = benchmarkToken,
                Progress = benchmark.Prorgess.Clone(),
            };
            
        }

        public class StartBenchmarkArgs
        {
            public string MountPath { get; set; }
            public int WorkingSet { get; set; }
            public int RandomAccessDuration { get; set; }
            public bool DisableODirect { get; set; }
            public int BlockSize { get; set; }
            public int Threads { get; set; }
        }

        public class BenchmarkResponse
        {
            public Guid Token;
            public ProgressInfo Progress;
        }
    }
}