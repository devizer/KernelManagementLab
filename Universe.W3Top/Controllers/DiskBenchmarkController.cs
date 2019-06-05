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
            
            throw new NotImplementedException();
        }

        [HttpPost, Route("get-progress")]
        public BenchmarkResponse GetProgress(Guid benchmarkToken)
        {
            throw new NotImplementedException();
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