using System;
using System.Linq;
using Universe.DiskBench;

namespace Universe.Dashboard.DAL
{
    public class DiskBenchmarkDataAccess
    {
        private DashboardContext _DbContext;

        public DiskBenchmarkDataAccess(DashboardContext dbContext)
        {
            _DbContext = dbContext;
        }

        public class DiskBenchmarkResult
        {
            public ProgressInfo Report { get; set; }
            public string ErrorInfo { get; set; }
            public bool IsFailed => !string.IsNullOrEmpty(ErrorInfo);
        }

        public DiskBenchmarkResult GetDiskBenchmarkResult(Guid benchmarkToken)
        {
            var query =
                from b in _DbContext.DiskBenchmark
                where b.Token == benchmarkToken
                select new {b.Report, b.ErrorInfo};

            var ret = query.FirstOrDefault();
            return ret == null
                ? null
                : new DiskBenchmarkResult()
                {
                    Report = ret.Report,
                    ErrorInfo = ret.ErrorInfo
                };
        }
    }
    
    
}