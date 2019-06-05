using System;
using System.Collections.Generic;
using System.Linq;
using KernelManagementJam;
using KernelManagementJam.Benchmarks;
using Microsoft.AspNetCore.Mvc;
using Universe.Benchmark.DiskBench;
using Universe.Dashboard.Agent;
using Universe.DiskBench;

namespace ReactGraphLab.Controllers
{
    [Route("api/benchmark/disk")]
    public class DiskBenchmarkController
    {
        private DiskBenchmarkQueue Queue;

        public DiskBenchmarkController(DiskBenchmarkQueue queue)
        {
            Queue = queue;
        }

        [HttpGet, Route("get-disks")]
        public List<DriveDetails> GetList()
        {
            return MountsDataSource.Mounts.FilterForHuman().OrderBy(x => x.MountEntry.MountPath).ToList();
        }

        [HttpPost, Route("start")]
        BenchmarkResponse StartBenchmark(StartBenchmarkArgs options)
        {
            DiskBenchmark diskBenchmark = new DiskBenchmark(options.MountPath,
                fileSize: options.WorkingSet*1024L*1024L,
                DataGeneratorFlavour.ILCode,
                options.BlockSize,
                options.RandomAccessDuration,
                options.DisableODirect && false,
                options.Threads
                );
            
            Guid token = Guid.NewGuid();
            Queue.Enqueue(token, diskBenchmark);
            return new BenchmarkResponse()
            {
                Token = token,
                Progress = diskBenchmark.Prorgess.Clone(),
            };
        }

        [HttpPost, Route("get-progress")]
        public BenchmarkResponse GetProgress(Guid benchmarkToken)
        {
            var benchmark = Queue.Find(benchmarkToken);
            if (benchmark == null)
            {
                return new BenchmarkResponse()
                {
                    Token = benchmarkToken,
                    Progress = null,
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
            public string MountPath;
            public int WorkingSet;
            public int RandomAccessDuration;
            public bool DisableODirect;
            public int BlockSize;
            public int Threads;
        }

        public class BenchmarkResponse
        {
            public Guid Token;
            public ProgressInfo Progress;
        }
    }
}