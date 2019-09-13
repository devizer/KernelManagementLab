using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace MyBenchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<StandardVsCustomSerializer>();
        }

    }
}