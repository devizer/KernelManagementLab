using System;
using System.Collections.Generic;
using System.Linq;
using Universe.DiskBench;

namespace Universe.Dashboard.DAL
{
    public class DiskBenchmarkDataAccess
    {
        public class DiskBenchmarkResult
        {
            public ProgressInfo Report { get; set; }
            public string ErrorInfo { get; set; }
            public bool IsFailed => !string.IsNullOrEmpty(ErrorInfo);
        }

        private DashboardContext _DbContext;

        public DiskBenchmarkDataAccess(DashboardContext dbContext)
        {
            _DbContext = dbContext;
        }

        public DiskBenchmarkResult GetDiskBenchmarkResult(Guid benchmarkToken)
        {
            var ret = DbResilience.Query(
                $"Query DiskBenchmark result for {benchmarkToken}",
                () => GetDiskBenchmarkResult_Impl(benchmarkToken),
                totalMilliseconds: 420,
                retryCount: 4
            );

            return ret;
        }

        private DiskBenchmarkResult GetDiskBenchmarkResult_Impl(Guid benchmarkToken)
        {
            var query =
                from b in _DbContext.DiskBenchmark
                where b.Token == benchmarkToken
                select new {b.Report, b.ErrorInfo};

            var result = query.FirstOrDefault();
            return result == null
                ? null
                : new DiskBenchmarkResult()
                {
                    Report = result.Report,
                    ErrorInfo = result.ErrorInfo
                };
        }

        public List<DiskBenchmarkEntity> GetHistory()
        {
            return DbResilience.Query(
                $"Query DiskBenchmark full history",
                () =>
                {
                    var query =
                        from b in _DbContext.DiskBenchmark
                        where b.ErrorInfo == null
                        orderby b.CreatedAt descending
                        select b;

                    return query.ToList();
                },
                totalMilliseconds: 250,
                retryCount: 4
            );
        }
    }
    
    public class DiskBenchmarkHistoryRow
    {
        public Guid Token { get; set; } // It is used for tests only
        public DateTime CreatedAt { get; set; }
        public string MountPath { get; set; }
        public long WorkingSetSize { get; set; }
        public string O_Direct { get; set; } // "" (disabled) | "True" (present) | "False" (absent)
        public double? Allocate { get; set; }
        public double? SeqRead { get; set; }
        public double? SeqWrite { get; set; }
        public int RandomAccessBlockSize { get; set; }
        public int ThreadsNumber { get; set; }
        public double? RandRead1T { get; set; } 
        public double? RandWrite1T { get; set; } 
        public double? RandReadNT { get; set; } 
        public double? RandWriteNT { get; set; } 
    }

    public static class DiskBenchmarkDataAccessExtensions
    {
        public static DiskBenchmarkHistoryRow ToHistoryItem(this DiskBenchmarkEntity benchmark)
        {
            // TODO: Build history row on SaveChanges and store as additional column
            ProgressStep GetStep(ProgressStepHistoryColumn column) => benchmark.Report.Steps.FirstOrDefault(step => step.Column == column);
            double? GetSpeed(ProgressStepHistoryColumn column) => GetStep(column)?.AvgBytesPerSecond;
            
            return new DiskBenchmarkHistoryRow()
            {
                Token = benchmark.Token,
                CreatedAt = benchmark.CreatedAt,
                MountPath = benchmark.Args.WorkFolder,
                WorkingSetSize = benchmark.Args.WorkingSetSize,
                O_Direct = Convert.ToString(GetStep(ProgressStepHistoryColumn.CheckODirect)?.Value),
                Allocate = GetSpeed(ProgressStepHistoryColumn.Allocate),
                SeqRead = GetSpeed(ProgressStepHistoryColumn.SeqRead),
                SeqWrite = GetSpeed(ProgressStepHistoryColumn.SeqWrite),
                RandomAccessBlockSize = benchmark.Args.RandomAccessBlockSize,
                ThreadsNumber = benchmark.Args.ThreadsNumber,
                RandRead1T = GetSpeed(ProgressStepHistoryColumn.RandRead1T),
                RandWrite1T = GetSpeed(ProgressStepHistoryColumn.RandWrite1T),
                RandReadNT = GetSpeed(ProgressStepHistoryColumn.RandReadNT),
                RandWriteNT = GetSpeed(ProgressStepHistoryColumn.RandWriteNT),
            };
        }
    }

}

