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
using Universe.FioStream.Binaries;

namespace Universe.W3Top.Controllers
{
    [Route("api/benchmark/disk")]
    [ApiController]
    public class DiskBenchmarkController : ControllerBase 
    {
        private DiskBenchmarkQueue Queue;
        public DiskBenchmarkDataAccess DbAccess;
        public readonly FioEnginesProvider FioEnginesProvider;

        public DiskBenchmarkController(DiskBenchmarkQueue queue, DiskBenchmarkDataAccess dbAccess, FioEnginesProvider fioEnginesProvider)
        {
            Queue = queue;
            DbAccess = dbAccess;
            FioEnginesProvider = fioEnginesProvider;
        }
        
        /*
        public DiskBenchmarkController(DiskBenchmarkQueue queue, DiskBenchmarkDataAccess dbAccess)
        {
            Queue = queue;
            DbAccess = dbAccess;
        }
        */

        [HttpGet, Route("get-disks")]
        public List<DriveDetails> GetList()
        {
            return MountsDataSource.Mounts.FilterForHuman().OrderBy(x => x.MountEntry.MountPath).ToList();
        }

        private DriveDetails FindDriveDetails(string mountPath)
        {
            return MountsDataSource.Mounts
                .Where(x => x.MountEntry.MountPath.StartsWith(mountPath))
                .OrderBy(x => x.MountEntry.MountPath.Length)
                .FirstOrDefault();
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
                    ? (IDiskBenchmark) new FioDiskBenchmark(Parameters) { Executable = "fio" } // DiskBenchmark(Parameters)
                    : (IDiskBenchmark) new ReadonlyDiskBenchmark(Parameters, MountsDataSource.Mounts);
            
            Guid token = Guid.NewGuid();
            var fileSystem = FindDriveDetails(Parameters.WorkFolder)?.MountEntry.FileSystem;
            var diskbenchmarkEnvironment = new DiskbenchmarkEnvironment() { FileSystems = fileSystem};
            Queue.Enqueue(token, diskBenchmark, diskbenchmarkEnvironment);
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
                // incomplete
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
                // archived
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
        public List<DiskBenchmarkHistoryRow> GetDiskBenchmarkHistory()
        {
            List<DiskBenchmarkEntity> entities = this.DbAccess.GetHistory();
            return entities.Select(x => x.ToHistoryItem()).ToList();
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