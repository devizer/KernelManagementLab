using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace KernelManagementJam.Benchmarks
{
    public class DataGenerator
    {
        public readonly DataFlavour Flavour;

        public DataGenerator(DataFlavour flavour)
        {
            Flavour = flavour;
        }

        public void NextBytes(byte[] arg)
        {
            switch (Flavour)
            {
                case DataFlavour.Random:
                case DataFlavour.StableRandom:
                    Random rand = Flavour == DataFlavour.StableRandom ? new Random(42) : new Random();
                    rand.NextBytes(arg);
                    return;
                
                case DataFlavour.FortyTwo:
                    Fill42(arg);
                    return;
                
                case DataFlavour.LoremIpsum:
                case DataFlavour.StableLoremIpsum:
                    FillLoremIpsum(arg, Flavour == DataFlavour.StableLoremIpsum);
                    return;
                
                case DataFlavour.ILCode:
                    FillIlCode(arg);
                    return;
                
                default:
                    throw new ArgumentException($"Flavour {Flavour} is not supported");
            }
        }

        static unsafe void FillIlCode(byte[] arg)
        {
            var file = Assembly.GetExecutingAssembly().Location;
            int copyLength;
            byte[] copy;
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                copyLength = (int) Math.Min(arg.Length, fs.Length);
                copy = new byte[copyLength];
                int readBytes = fs.Read(copy, 0, copyLength);

                int length = arg.Length;
                fixed (byte* ptrFrom = &copy[0], ptrTo = &arg[0])
                {
                    byte* dst = ptrTo;
                    while (length > 0)
                    {
                        Copy(ptrFrom, dst, Math.Min(readBytes, length));
                        length -= readBytes;
                        dst += readBytes;
                    }
                }
            }
        }

        static void FillLoremIpsum(byte[] arg, bool isStable)
        {
            string[] words = new[]{"Lorem", "Ipsum", "Dolor", "Sit", "Amet", "Consectetuer",
                "Adipiscing", "Elit", "Sed", "Diam", "Nonummy", "Nibh", "Euismod",
                "Tincidunt", "Ut", "Laoreet", "Dolore", "Magna", "Aliquam", "Erat"};
            
            Random rand = isStable ? new Random(42) : new Random();
            int count = 0, length = arg.Length, wordsCount = words.Length;
            StringBuilder b = new StringBuilder(length);
            while (count < length)
            {
                if (count > 0) b.Append(' ');
                var word = words[rand.Next(wordsCount)];
                b.Append(word);
                count += word.Length;
            }

            byte[] copy = Encoding.ASCII.GetBytes(b.ToString());
            Copy(copy, arg, length);
        }

        static unsafe void Copy(byte[] from, byte[] to, int count)
        {
            fixed(byte* ptrFrom = &from[0], ptrTo = &to[0])
                Copy(ptrFrom, ptrTo, count);
        }
        
        static unsafe void Copy(byte* ptrFrom, byte* ptrTo, int count)
        {
            var src = ptrFrom;
            var dst = ptrTo;
            var len = count;

            while (len >= 8)
            {
                *(long*)dst = *(long*)src;

                src += 8;
                dst += 8;
                len -= 8;
            }

            while (len > 0)
            {
                *dst = *src;

                src++;
                dst++;
                len--;
            }

        }


        static unsafe void Fill42(byte[] arg)
        {
            fixed (byte* ptr = &arg[0])
            {
                long* l = (long*) ptr;
                int count = arg.Length / 8;
                while (count-- > 0)
                {
                    *l++ = 0x2A0000002AL;
                }
            }
        }
    }

    public enum DataFlavour
    {
        Random,
        StableRandom,
        
        // Text
        LoremIpsum,
        StableLoremIpsum,
        
        // 42 (maximum compression)
        FortyTwo,
        
        // An MS IL binary 
        ILCode
    }
}