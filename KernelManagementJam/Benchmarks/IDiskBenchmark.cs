using System;
using System.Runtime.Serialization;
using Universe.DiskBench;

namespace Universe.Benchmark.DiskBench
{
    public interface IDiskBenchmark
    {
        DiskBenchmarkOptions Parameters { get; }
        ProgressInfo Progress { get; }
        void Perform();
        
        bool IsCanceled { get; }
        void RequestCancel();
    }

    [Serializable]
    public class BenchmarkCanceledException : Exception
    {
        public BenchmarkCanceledException()
        {
        }

        protected BenchmarkCanceledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BenchmarkCanceledException(string message) : base(message)
        {
        }

        public BenchmarkCanceledException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}