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
                        where b.IsSuccess
                        orderby b.CreatedAt descending
                        select b;

                    return query.ToList();
                },
                totalMilliseconds: 250,
                retryCount: 4
            );
        }
    }
    
}

