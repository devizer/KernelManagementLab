using System;
using System.Collections.Generic;
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
        public void All(DataFlavour fla)
        {
            Console.WriteLine($"Flavour : {fla}");
            // return;

            var sizes = new[] {1, 2, 3, 4, 8, 16, 1024 * 1024, 128 * 1024 * 1024};
            foreach (var size in sizes)
            {
                DataGenerator dg = new DataGenerator(fla);
                byte[] bytes = new byte[size];
                dg.NextBytes(bytes);
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