using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JsonLab;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class LongArrayConverter_Tests
    {

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
        }
        
        [Test]
        public void Tests()
        {
            List<long> cases = new List<long>() {0, long.MinValue, long.MinValue + 1, long.MaxValue, long.MaxValue - 1,};
            decimal cur = 1m;
            while (cur <= (decimal) long.MaxValue)
            {
                var next = (long) cur;
                if (next != cases.Last())
                {
                    cases.Add(-next + (next % 10));
                    cases.Add(-next + 1);
                    cases.Add(-next);
                    cases.Add(next - (next % 10));
                    cases.Add(next - 1);
                    cases.Add(next);
                }
                cur = cur * 1.042m;
            }
            
            Console.WriteLine($"Cases: {cases.Count} numbers [{string.Join(",", cases)}]");

            for (int len = 0; len <= 2; len++)
            {
                foreach (long @case in cases)
                {
                    long[] data = new long[len];
                    for (int i = 0; i < len; i++) data[i] = @case;
                    var src = data;

                    var original = Serialize(src, null);
                    var new1 = Serialize(src, LongArrayConverter.SlowerInstance);
                    var new2Heapless = Serialize(src, LongArrayConverter.Instance);

                    Assert.AreEqual(original, new1, $"LongArrayConverter.SlowerInstance case is {@case} * {len} times");
                    Assert.AreEqual(original, new2Heapless, $"LongArrayConverter.Instance case is {@case} * {len} times");
                }
            }
        }
        
        
        private string Serialize(object data, JsonConverter optionalConverter = null)
        {
            JsonSerializer ser = new JsonSerializer()
            {
                Formatting = Formatting.None,
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
            ser.Serialize(jwr, data);
            jwr.Flush();

            return json.ToString();
        }
        
    }
}