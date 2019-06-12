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
        public DiskBenchmarkDataAccess DbAccess;

        public DiskBenchmarkController(DiskBenchmarkQueue queue, DiskBenchmarkDataAccess dbAccess)
        {
            Queue = queue;
            DbAccess = dbAccess;
        }

        [HttpGet, Route("get-disks")]
        public List<DriveDetails> GetList()
        {
            return MountsDataSource.Mounts.FilterForHuman().OrderBy(x => x.MountEntry.MountPath).ToList();
        }

        [HttpPost, Route("start-disk-benchmark")]
        public BenchmarkResponse StartBenchmark(StartBenchmarkArgs options)
        {
            // return new BenchmarkResponse() {Token = Guid.NewGuid()};
            Console.WriteLine($"StartBenchmark options: {options.AsJson()}");

            DiskBenchmarkOptions Parameters = new DiskBenchmarkOptions()
            {
                WorkFolder = options.MountPath,
                WorkingSetSize = options.WorkingSet * 1024L * 1024L,
                Flavour = DataGeneratorFlavour.ILCode,
                RandomAccessBlockSize = options.BlockSize,
                StepDuration = options.RandomAccessDuration * 1000,
                DisableODirect = options.DisableODirect && false,
                ThreadsNumber = options.Threads,
            };

            DiskBenchmark diskBenchmark = new DiskBenchmark(
                Parameters
            );
            
            Guid token = Guid.NewGuid();
            Queue.Enqueue(token, diskBenchmark);
            return new BenchmarkResponse()
            {
                Token = token,
                Progress = diskBenchmark.Progress.Clone(),
            };
        }

        [HttpPost, Route("get-disk-benchmark-progress-{benchmarkToken}")]
        public BenchmarkResponse GetProgress(Guid benchmarkToken)
        {
            var enlistedBenchmark = Queue.Find(benchmarkToken);
            if (enlistedBenchmark.Index >= 0)
            {
                var progress = enlistedBenchmark.Benchmark.Progress.Clone();
                if (enlistedBenchmark.Index > 0)
                {
                    // add Waiting in queue step
                    ProgressStep waitingStep =
                        new ProgressStep($"Waiting in queue at position {(enlistedBenchmark.Index + 0)}")
                        {
                            State = ProgressStepState.InProgress,
                        };

                    progress.Steps.Insert(0, waitingStep);
                }

                return new BenchmarkResponse()
                {
                    Token = benchmarkToken,
                    Progress = progress,
                };
            }

            else
            {
                var benchmarkResult = DbAccess.GetDiskBenchmarkResult(benchmarkToken);
                return new BenchmarkResponse()
                {
                    Token = benchmarkToken,
                    Progress = benchmarkResult?.Report,
                    ErrorInfo = benchmarkResult == null ? "Unknown Benchmark Session" : benchmarkResult.ErrorInfo 
                };
            }
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
            public string ErrorInfo;
        }
    }
}