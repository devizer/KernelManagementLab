using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Running;

namespace MyBenchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var xxx = new[] {1L}.ToList().ToImmutableArray();
            var yyy = xxx.GetType();
            Console.WriteLine($"ToImmutableArray: {yyy}");
            Console.WriteLine(long.MaxValue);
            Console.WriteLine(long.MinValue);
            // GenerateToBuffer();
            DebugCustomSerializer();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            long lll = 12345678987654321L;
            Console.WriteLine(lll);
            Console.WriteLine(Convert.ToString(lll));

            var summary = BenchmarkRunner.Run<StandardVsCustomSerializer>();
            return;
            // var summary2 = BenchmarkRunner.Run<Md5VsSha256>();

        }

        private static void DebugCustomSerializer()
        {
            StandardVsCustomSerializer ser = new StandardVsCustomSerializer()
            {
                Minify = false,
                ArraysCount = 2
            };
            ser.Setup();

            string jsonStandard = ser.Default();
            string jsonCustom = ser.Optimized();
            Console.WriteLine(jsonCustom);
        }

        static void GenerateToBuffer()
        {
            int len = long.MaxValue.ToString("0").Length;
            Console.WriteLine("byte " + string.Join(",", Enumerable.Range(0,len+1).Select(x => $"p{x}=0")) + ";");
            for (int i = 0; i <= len; i++)
            {
                Console.WriteLine($"if (arg != 0) {{ p{i} = (byte)(arg % 10); arg = arg / 10; ");
            }
/*
            for (int i = len; i >=0; i--)
            {
                Console.Write($" }} else p{i}=0; ");
            }
            Console.WriteLine();
*/

            Console.WriteLine(new string('}', len+1));
            Console.WriteLine("bool hasMeaning = false;");
            for (int i = len; i >= 0; i--)
            {
                Console.WriteLine($"if (!hasMeaning && p{i} != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p{i}));");
            }
        }
    }
}