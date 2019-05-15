using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using JsonLab;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyBenchmarks
{
    [CoreJob]
    [RankColumn]
    [MemoryDiagnoser]
    public class StandardVsCustomSerializer
    {
        public enum CollectionFlavour
        {
            List,
            Array,
            ROList,
            ROArray,
        }
        
        private dynamic RootData;

        [Params(20)]
        public int ArraysCount;
        
        [Params(true, false)]
        public bool Minify;

        [Params(CollectionFlavour.List, CollectionFlavour.Array, CollectionFlavour.ROList, CollectionFlavour.ROArray)]
        public CollectionFlavour Kind;

        private long[] TheLongs = new[] { 0, 1L, 12L, 123L, 1234L, 12345678987654321L, -1L, -12L, -123L, -1234L, -12345678987654321L };

        [GlobalSetup]
        public void Setup()
        {
            List<object> list = new List<object>();
            for (int i = 0; i < ArraysCount; i++)
            {
                var item = TheLongs.Concat(Enumerable.Range(0, 61).Select(x => 42L));
                if (Kind == CollectionFlavour.List) list.Add(item.ToList());
                else if (Kind == CollectionFlavour.Array) list.Add(item.ToArray());
                else if (Kind == CollectionFlavour.ROList) list.Add(item.ToList().ToImmutableList());
                else if (Kind == CollectionFlavour.ROArray) list.Add(item.ToList().ToImmutableArray());
                else throw new InvalidOperationException($"Unknown flavour: {Kind}");
            }

            RootData = list.ToArray();
        }

        [Benchmark]
        public string Optimized()
        {
            return Serialize(optionalConverter: LongArrayConverter.Instance);
        }

        [Benchmark]
        public string OptimizedHeapless()
        {
            return Serialize(optionalConverter: LongArrayConverter.HeaplessInstance);
        }

        [Benchmark]
        public string Default()
        {
            return Serialize();
        }

        private string Serialize(JsonConverter optionalConverter = null)
        {
            JsonSerializer ser = new JsonSerializer()
            {
                Formatting = !Minify ? Formatting.Indented : Formatting.None,
            };

            if (optionalConverter != null)
                ser.Converters.Add(optionalConverter);

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
            ser.Serialize(jwr, RootData);
            jwr.Flush();

            return json.ToString();
        }

    }
}