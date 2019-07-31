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

        public static readonly LongArrayConverter Instance = new LongArrayConverter();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                StringBuilder stringBuilder;
                if (value is long[] arr)
                {
                    int len = arr.Length;
                    stringBuilder = new StringBuilder(len << 1);
                    int pos = 0;
                    for (int i = 0; i < len; i++)
                    {
                        if (pos++ != 0) stringBuilder.Append(',');
                        HeaplessAppend(stringBuilder, arr[i]);
                    }
                }
                else if (value is List<long> list)
                {
                    int len = list.Count;
                    stringBuilder = new StringBuilder(len << 1);
                    for(int pos=0; pos < len; pos++)
                    {
                        if (pos != 0) stringBuilder.Append(',');
                        HeaplessAppend(stringBuilder, list[pos]);
                    }
                }
                else if (value is ICollection<long> collection)
                {
                    int len = collection.Count;
                    stringBuilder = new StringBuilder(len << 1);
                    int pos = 0;
                    foreach (long item in collection)
                    {
                        if (pos++ != 0) stringBuilder.Append(',');
                        HeaplessAppend(stringBuilder, item);
                    }
                }
                else if (value is IEnumerable<long> enumerable)
                {
                    stringBuilder = new StringBuilder();
                    int pos = 0;
                    foreach (long item in enumerable)
                    {
                        if (pos++ != 0) stringBuilder.Append(',');
                        HeaplessAppend(stringBuilder, item);
                    }
                }
                else
                {
                    throw new InvalidOperationException("LongArrayConverter.CanConvert does not work properly. Report it.");
                }

                writer.WriteStartArray();
                writer.WriteRaw(stringBuilder.ToString());
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
            return objectType == ArrayType || typeof(IEnumerable<long>).IsAssignableFrom(objectType);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void HeaplessAppend(StringBuilder builder, long arg)
        {
            if (arg == 9223372036854775807L)
            {
                builder.Append("9223372036854775807");
                return;
            }

            if (arg == -9223372036854775808L)
            {
                builder.Append("-9223372036854775808");
                return;
            }

            if (arg < 0)
            {
                arg = -arg;
                builder.Append('-');
            }

            if (arg < 10)
            {
                builder.Append((char)(48 + arg));
            }
            else if (arg < 100)
            {
                builder.Append((char)(48 + (arg / 10)));
                builder.Append((char)(48 + (arg % 10)));
            }
            else if (arg < 1000)
            {
                builder.Append((char)(48 + arg / 100));
                builder.Append((char)(48 + (arg / 10) % 10));
                builder.Append((char)(48 + arg % 10));
            }
            else if (arg < 10000)
            {
                var p0 = arg % 10;
                var p1a = arg / 10;
                var p1 = p1a % 10;
                var p2a = p1a / 10;
                var p2 = p2a % 10;
                var p3 = p2a / 10;
                builder.Append((char)(48 + p3));
                builder.Append((char)(48 + p2));
                builder.Append((char)(48 + p1));
                builder.Append((char)(48 + p0));
            }
            else
            {
                
                byte p0 = (byte) (arg % 10);
                arg = arg / 10;
                
                if (arg != 0)
                {
                    byte p1 = (byte) (arg % 10);
                    arg = arg / 10;
                    
                    if (arg != 0)
                    {
                        byte p2 = (byte) (arg % 10);
                        arg = arg / 10;
                        
                        if (arg != 0)
                        {
                            byte p3 = (byte) (arg % 10);
                            arg = arg / 10;
                            
                            if (arg != 0)
                            {
                                byte p4 = (byte) (arg % 10);
                                arg = arg / 10;
                                
                                if (arg != 0)
                                {
                                    byte p5 = (byte) (arg % 10);
                                    arg = arg / 10;
                                    
                                    if (arg != 0)
                                    {
                                        byte p6 = (byte) (arg % 10);
                                        arg = arg / 10;
                                        
                                        if (arg != 0)
                                        {
                                            byte p7 = (byte) (arg % 10);
                                            arg = arg / 10;
                                            
                                            if (arg != 0)
                                            {
                                                byte p8 = (byte) (arg % 10);
                                                arg = arg / 10;
                                                
                                                if (arg != 0)
                                                {
                                                    byte p9 = (byte) (arg % 10);
                                                    arg = arg / 10;
                                                    
                                                    if (arg != 0)
                                                    {
                                                        byte p10 = (byte) (arg % 10);
                                                        arg = arg / 10;
                                                        
                                                        if (arg != 0)
                                                        {
                                                            byte p11 = (byte) (arg % 10);
                                                            arg = arg / 10;
                                                            
                                                            if (arg != 0)
                                                            {
                                                                byte p12 = (byte) (arg % 10);
                                                                arg = arg / 10;
                                                                
                                                                if (arg != 0)
                                                                {
                                                                    byte p13 = (byte) (arg % 10);
                                                                    arg = arg / 10;
                                                                    
                                                                    if (arg != 0)
                                                                    {
                                                                        byte p14 = (byte) (arg % 10);
                                                                        arg = arg / 10;
                                                                        
                                                                        if (arg != 0)
                                                                        {
                                                                            byte p15 = (byte) (arg % 10);
                                                                            arg = arg / 10;
                                                                            
                                                                            if (arg != 0)
                                                                            {
                                                                                byte p16 = (byte) (arg % 10);
                                                                                arg = arg / 10;
                                                                                
                                                                                if (arg != 0)
                                                                                {
                                                                                    byte p17 = (byte) (arg % 10);
                                                                                    arg = arg / 10;
                                                                                    
                                                                                    if (arg != 0)
                                                                                    {
                                                                                        byte p18 = (byte) (arg % 10);
                                                                                        arg = arg / 10;
                                                                                        
                                                                                        if (arg != 0)
                                                                                        {
                                                                                            byte p19 = (byte) (arg % 10);
                                                                                            builder.Append((char) (48 + p19));
                                                                                        }
                                                                                        builder.Append((char) (48 + p18));
                                                                                    }
                                                                                    builder.Append((char) (48 + p17));
                                                                                }
                                                                                builder.Append((char) (48 + p16));
                                                                            }
                                                                            builder.Append((char) (48 + p15));
                                                                        }
                                                                        builder.Append((char) (48 + p14));
                                                                    }
                                                                    builder.Append((char) (48 + p13));
                                                                }                                
                                                                builder.Append((char) (48 + p12));
                                                            }
                                                            builder.Append((char) (48 + p11));
                                                        }
                                                        builder.Append((char) (48 + p10));
                                                    }
                                                    builder.Append((char) (48 + p9));
                                                }
                                                builder.Append((char) (48 + p8));
                                            }
                                            builder.Append((char) (48 + p7));
                                        }
                                        builder.Append((char) (48 + p6));
                                    }
                                    builder.Append((char) (48 + p5));
                                }
                                builder.Append((char) (48 + p4));
                            }
                            builder.Append((char) (48 + p3));
                        }
                        builder.Append((char) (48 + p2));
                    }
                    builder.Append((char) (48 + p1));
                }
                builder.Append((char) (48 + p0));
            }
        }
    }

}
