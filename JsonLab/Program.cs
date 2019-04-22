using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyBenchmarks
{
    [ClrJob(true), CoreJob, MonoJob]
    [RPlotExporter, RankColumn]
    public class StandardVsCustomSerializer
    {
        private dynamic data;

        [Params(20)]
        public int N;
        
        [Params(true, false)]
        public bool Minify;

        [GlobalSetup]
        public void Setup()
        {
            List<object> list = new List<object>();
            for (int i = 0; i < N; i++)
                data.Add(new List<long>(new long[61]));

            data = list;
        }

        [Benchmark]
        public string Standard()
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
//        public string Custom()
//        {
//            
//        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<StandardVsCustomSerializer>();
        }
    }
}