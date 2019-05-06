using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JsonLab;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyBenchmarks
{
    [CoreJob]
    [RankColumn]
    public class StandardVsCustomSerializer
    {
        private dynamic data;

        [Params(20)]
        public int N;
        
        [Params(true, false)]
        public bool Minify;

        private long[] VaryLongs = new[] { 0, 1L, 12L, 123L, 1234L, 12345678987654321L, -1L, -12L, -123L, -1234L, -12345678987654321L };

        [GlobalSetup]
        public void Setup()
        {
            List<object> list = new List<object>();
            for (int i = 0; i < N; i++)
            {
                // list.Add(VaryLongs.Concat(Enumerable.Range(0, 61).Select(x => 42L)).ToList());
                list.Add(new long[] { 42L});
                
            }

            data = list;
        }

        [Benchmark]
        public string Optimized()
        {
            var converters = new JsonConverterCollection();
            JsonSerializer ser = new JsonSerializer()
            {
                Formatting = !Minify ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            ser.Converters.Add(LongArrayConverter.Instance);

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false,
                    ProcessDictionaryKeys = true,
                }
            };

            ser.ContractResolver = contractResolver;

            StringBuilder json = new StringBuilder();
            StringWriter jwr = new StringWriter(json);
            ser.Serialize(jwr, data);
            jwr.Flush();

            return json.ToString();
        }


        [Benchmark]
        public string Default()
        {
            JsonSerializer ser = new JsonSerializer()
            {
                Formatting = !Minify ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false,
                    ProcessDictionaryKeys = true,
                }
            };

            ser.ContractResolver = contractResolver;

            StringBuilder json = new StringBuilder();
            StringWriter jwr = new StringWriter(json);
            ser.Serialize(jwr, data);
            jwr.Flush();

            return json.ToString();
        }

//        [Benchmark]
//        public string Optimized()
//        {
//            
//        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
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
                N = 2
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