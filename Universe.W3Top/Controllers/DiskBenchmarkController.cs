using System;
using System.Collections;
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

namespace Universe.W3Top.Controllers
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
        public BenchmarkProgressResponse StartBenchmark(StartBenchmarkArgs options)
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

            bool hasWritePermission = DiskBenchmarkChecks.HasWritePermission(Parameters.WorkFolder);
            IDiskBenchmark diskBenchmark =
                hasWritePermission
                    ? (IDiskBenchmark) new DiskBenchmark(Parameters)
                    : (IDiskBenchmark) new ReadonlyDiskBenchmark(Parameters, MountsDataSource.Mounts);
            
            Guid token = Guid.NewGuid();
            Queue.Enqueue(token, diskBenchmark);
            return new BenchmarkProgressResponse()
            {
                Token = token,
                Progress = diskBenchmark.Progress.Clone(),
            };
        }

        [HttpPost, Route("get-disk-benchmark-progress-{benchmarkToken}")]
        public BenchmarkProgressResponse GetProgress(Guid benchmarkToken)
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

                return new BenchmarkProgressResponse()
                {
                    Token = benchmarkToken,
                    Progress = progress,
                };
            }

            else
            {
                DiskBenchmarkDataAccess.DiskBenchmarkResult benchmarkResult = DbAccess.GetDiskBenchmarkResult(benchmarkToken);
                return new BenchmarkProgressResponse()
                {
                    Token = benchmarkToken,
                    Progress = benchmarkResult?.Report,
                    ErrorInfo = benchmarkResult == null ? "Unknown Benchmark Session" : benchmarkResult.ErrorInfo 
                };
            }
        }

        [HttpPost, Route("cancel-disk-benchmark-{benchmarkToken}")]
        public void Cancel(Guid benchmarkToken)
        {
            Queue.Cancel(benchmarkToken);
        }

        [HttpPost, Route("get-disk-benchmark-history")]
        public IList GetDiskBenchmarkHistory()
        {
            List<DiskBenchmarkEntity> entities = this.DbAccess.GetHistory();
            double? GetSpeed(DiskBenchmarkEntity entity, ProgressStepHistoryColumn column) => entity.Report.Steps.FirstOrDefault(step => step.Column == column)?.AvgBytesPerSecond;

            return entities.Select(benchmark => new
            {
                MountPath = benchmark.Args.WorkFolder,
                WorkingSetSize = benchmark.Args.WorkingSetSize,
                O_Direct = Convert.ToString(benchmark.Report.Steps.FirstOrDefault(step => step.Column == ProgressStepHistoryColumn.CheckODirect)?.Value),
                Allocate = benchmark.Report.Steps.FirstOrDefault(step => step.Column == ProgressStepHistoryColumn.Allocate)?.AvgBytesPerSecond,
                SeqRead = benchmark.Report.Steps.FirstOrDefault(step => step.Column == ProgressStepHistoryColumn.SeqRead)?.AvgBytesPerSecond,
                SeqWrite = GetSpeed(benchmark, ProgressStepHistoryColumn.SeqWrite),
                RandomAccessBlockSize = benchmark.Args.RandomAccessBlockSize,
                ThreadsNumber = benchmark.Args.ThreadsNumber,
                RandRead1T = benchmark.Report.Steps.FirstOrDefault(step => step.Column == ProgressStepHistoryColumn.RandRead1T)?.AvgBytesPerSecond,
                RandWrite1T = benchmark.Report.Steps.FirstOrDefault(step => step.Column == ProgressStepHistoryColumn.RandWrite1T)?.AvgBytesPerSecond,
                RandReadNT = benchmark.Report.Steps.FirstOrDefault(step => step.Column == ProgressStepHistoryColumn.RandReadNT)?.AvgBytesPerSecond,
                RandWriteNT = benchmark.Report.Steps.FirstOrDefault(step => step.Column == ProgressStepHistoryColumn.RandWriteNT)?.AvgBytesPerSecond,
            }).ToList();

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

        public class BenchmarkProgressResponse
        {
            public Guid Token;
            public ProgressInfo Progress;
            public string ErrorInfo;
        }
    }
}