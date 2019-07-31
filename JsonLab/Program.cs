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

        static void GenerateHeapless()
        {
            int len = long.MaxValue.ToString("0").Length;
            Console.WriteLine("byte " + string.Join(",", Enumerable.Range(0,len+1).Select(x => $"p{x}=0")) + ";");
            for (int i = 0; i <= len; i++)
            {
                Console.WriteLine($"if (arg != 0) {{ p{i} = (byte)(arg % 10); arg = arg / 10; ");
            }

            Console.WriteLine(new string('}', len+1));
            Console.WriteLine("bool hasMeaning = false;");
            for (int i = len; i >= 0; i--)
            {
                Console.WriteLine($"if (!hasMeaning && p{i} != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p{i}));");
            }
        }
    }
}