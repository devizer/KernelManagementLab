using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace JsonLab
{
    public class LongArrayConverter : JsonConverter
    {
        private readonly Type ArrayType = typeof(long[]);
        private readonly Type ListType = typeof(List<long>);

        public static readonly LongArrayConverter Instance = new LongArrayConverter();

        public LongArrayConverter()
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                StringBuilder b;
                if (value is long[] arr)
                {
                    int l = arr.Length;
                    int lastIndex = l - 1;
                    b = new StringBuilder(l + l);
                    for (int i = 0; i < l; i++)
                    {
                        ToBuilder(b, arr[i]);
                        if (i < lastIndex) b.Append(',');
                    }
                }
                else if (value is ICollection<long> collection)
                {
                    int l = collection.Count;
                    int lastIndex = l - 1;
                    int i = 0;
                    b = new StringBuilder(l + l);
                    foreach (long item in collection)
                    {
                        ToBuilder(b, item);
                        if (i++ < lastIndex) b.Append(',');
                    }
                }
                else
                {
                    throw new InvalidOperationException("Report It");
                }

                writer.WriteStartArray();
                writer.WriteRaw(b.ToString());
                writer.WriteEndArray();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            // Console.WriteLine($"CAN CONVERT: {objectType}");
            return objectType == ArrayType || typeof(ICollection<long>).IsAssignableFrom(objectType);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ToBuilder(StringBuilder b, long arg)
        {
            if (arg == 9223372036854775807L)
            {
                b.Append("9223372036854775807");
                return;
            }

            if (arg == -9223372036854775808L)
            {
                b.Append("-9223372036854775808");
                return;
            }

            if (arg < 0)
            {
                arg = -arg;
                b.Append('-');
            }

            if (arg == 0)
                b.Append('0');

            else if (arg < 10)
            {
                b.Append((char)(48 + arg));
            }
            else if (arg < 100)
            {
                b.Append((char)(48 + (arg / 10)));
                b.Append((char)(48 + (arg % 10)));
            }
            else if (arg < 1000)
            {
                b.Append((char)(48 + arg / 100));
                b.Append((char)(48 + (arg / 10) % 10));
                b.Append((char)(48 + arg % 10));
            }
            else if (arg < 10000)
            {
                var p3 = arg % 10;
                var p2a = arg / 10;
                var p2 = p2a % 10;
                var p1a = p2a / 10;
                var p1 = p1a % 10;
                var p0 = p1a / 10;
                b.Append((char)(48 + p0));
                b.Append((char)(48 + p1));
                b.Append((char)(48 + p2));
                b.Append((char)(48 + p3));
            }
            else
            {
                // b.Append(Convert.ToString(arg));
                // return;
                byte p0 = 0, p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0, p6 = 0, p7 = 0, p8 = 0, p9 = 0, p10 = 0, p11 = 0, p12 = 0, p13 = 0, p14 = 0, p15 = 0, p16 = 0, p17 = 0, p18 = 0, p19 = 0;
                if (arg != 0)
                {
                    p0 = (byte)(arg % 10); arg = arg / 10;
                    if (arg != 0)
                    {
                        p1 = (byte)(arg % 10); arg = arg / 10;
                        if (arg != 0)
                        {
                            p2 = (byte)(arg % 10); arg = arg / 10;
                            if (arg != 0)
                            {
                                p3 = (byte)(arg % 10); arg = arg / 10;
                                if (arg != 0)
                                {
                                    p4 = (byte)(arg % 10); arg = arg / 10;
                                    if (arg != 0)
                                    {
                                        p5 = (byte)(arg % 10); arg = arg / 10;
                                        if (arg != 0)
                                        {
                                            p6 = (byte)(arg % 10); arg = arg / 10;
                                            if (arg != 0)
                                            {
                                                p7 = (byte)(arg % 10); arg = arg / 10;
                                                if (arg != 0)
                                                {
                                                    p8 = (byte)(arg % 10); arg = arg / 10;
                                                    if (arg != 0)
                                                    {
                                                        p9 = (byte)(arg % 10); arg = arg / 10;
                                                        if (arg != 0)
                                                        {
                                                            p10 = (byte)(arg % 10); arg = arg / 10;
                                                            if (arg != 0)
                                                            {
                                                                p11 = (byte)(arg % 10); arg = arg / 10;
                                                                if (arg != 0)
                                                                {
                                                                    p12 = (byte)(arg % 10); arg = arg / 10;
                                                                    if (arg != 0)
                                                                    {
                                                                        p13 = (byte)(arg % 10); arg = arg / 10;
                                                                        if (arg != 0)
                                                                        {
                                                                            p14 = (byte)(arg % 10); arg = arg / 10;
                                                                            if (arg != 0)
                                                                            {
                                                                                p15 = (byte)(arg % 10); arg = arg / 10;
                                                                                if (arg != 0)
                                                                                {
                                                                                    p16 = (byte)(arg % 10); arg = arg / 10;
                                                                                    if (arg != 0)
                                                                                    {
                                                                                        p17 = (byte)(arg % 10); arg = arg / 10;
                                                                                        if (arg != 0)
                                                                                        {
                                                                                            p18 = (byte)(arg % 10); arg = arg / 10;
                                                                                            if (arg != 0)
                                                                                            {
                                                                                                p19 = (byte)(arg % 10); 
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                bool hasMeaning = false;
                if (!hasMeaning && p19 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p19));
                if (!hasMeaning && p18 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p18));
                if (!hasMeaning && p17 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p17));
                if (!hasMeaning && p16 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p16));
                if (!hasMeaning && p15 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p15));
                if (!hasMeaning && p14 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p14));
                if (!hasMeaning && p13 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p13));
                if (!hasMeaning && p12 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p12));
                if (!hasMeaning && p11 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p11));
                if (!hasMeaning && p10 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p10));
                if (!hasMeaning && p9 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p9));
                if (!hasMeaning && p8 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p8));
                if (!hasMeaning && p7 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p7));
                if (!hasMeaning && p6 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p6));
                if (!hasMeaning && p5 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p5));
                if (!hasMeaning && p4 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p4));
                if (!hasMeaning && p3 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p3));
                if (!hasMeaning && p2 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p2));
                if (!hasMeaning && p1 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p1));
                if (!hasMeaning && p0 != 0) hasMeaning = true; if (hasMeaning) b.Append((char)(48 + p0));
            }
        }
    }

}
