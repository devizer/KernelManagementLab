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
        public void Test_Null()
        {
            var src = new TypedCollection();
            var original = Serialize(src, null);
            var optimized = Serialize(src, LongArrayConverter.Instance);
            Console.WriteLine(original);
            Assert.AreEqual(original, optimized);
        }

        class TypedCollection
        {
            public long[] Content = null;
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
                cur *= 1.01m;
            }
            
            for (int len = 0; len <= 2; len++)
            {
                foreach (long testCase in cases)
                {
                    long[] data = new long[len];
                    for (int i = 0; i < len; i++) data[i] = testCase;
                    var src = data;

                    var original = Serialize(src, null);
                    var optimized = Serialize(src, LongArrayConverter.Instance);
                    Assert.AreEqual(original, optimized, $"LongArrayConverter.Instance case is {testCase} * {len} times");
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


            StringBuilder json = new StringBuilder();
            StringWriter jwr = new StringWriter(json);
            ser.Serialize(jwr, data);
            jwr.Flush();

            return json.ToString();
        }
        
    }
}