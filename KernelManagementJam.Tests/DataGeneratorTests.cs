using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using KernelManagementJam.Benchmarks;
using NUnit.Framework;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class DataGeneratorTests
    {
        [Test]
        [TestCaseSourceAttribute(typeof(DataGeneratorTests), nameof(GetAllFlavour))]
        public void Perform(DataFlavour fla)
        {
            Console.WriteLine($"Flavour : {fla}");
            // return;

            var sizes = new[] {1, 2, 3, 4, 8, 16, 1024 * 1024, 128 * 1024 * 1024};
            foreach (var size in sizes)
            {
                var bytes = Generate(fla, size);
            }
        }

        [Test]
        [TestCaseSourceAttribute(typeof(DataGeneratorTests), nameof(GetAllFlavour))]
        public void Measure(DataFlavour fla)
        {
            Console.WriteLine($"Flavour : {fla}");
            // return;

            var sizes = new[] {1, 2, 3, 4, 8, 16, 1024 * 1024, 128 * 1024 * 1024};
            foreach (var size in sizes)
            {
                var bytes = Generate(fla, size);
                int len = GetCompressedSize(bytes);
                Console.WriteLine($"{size:n0} --> {len:n0}, {(len * 100.000 / size):f1}%");
            }
        }

        private static byte[] Generate(DataFlavour fla, int size)
        {
            DataGenerator dg = new DataGenerator(fla);
            byte[] bytes = new byte[size];
            dg.NextBytes(bytes);
            return bytes;
        }

        static int GetCompressedSize(byte[] arg)
        {
            using (MemoryStream mem = new MemoryStream())
            using (DeflateStream pack = new DeflateStream(mem, CompressionLevel.Optimal))
            using (BufferedStream buf = new BufferedStream(pack, 65536))
            {
                buf.Write(arg, 0, arg.Length);
                buf.Flush();
                pack.Flush();
                return (int) mem.Length;
            }
        }

        public static DataFlavour[] GetAllFlavour()
        {
            DataFlavour[] flavours = Enum.GetValues(typeof(DataFlavour)).OfType<DataFlavour>().ToArray();
            // flavours = new[] {DataFlavour.Random, DataFlavour.StableRandom, DataFlavour.FortyTwo, DataFlavour.LoremIpsum, DataFlavour.ILCode};
            return flavours;
        }
        
    }
}