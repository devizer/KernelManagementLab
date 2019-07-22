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

    class StructuredColumns
    {
        /*
         * Flavour for write
         * long WorkingSetSize
         * int ThreadsNumber
         * int RandomAccessBlockSize
         * bool DisableODirect
         * 
         * bool [has] O_DIRECT
         * === NUMBERS (bytes/sec) ===
         * Allocate,
         * Seq Read,
         * Seq Write (optional),
         * Random Read N Threads
         * Random Write N Threads (optional)
         */
        
        /*
         * Seq: WorkingSetSize, Allocate, Seq Read, Seq Write
         * Random: RandomAccessBlockSize, threads number, Read 1T, Write 1T, Read N, Write N
         */
    }
}