using System;
using System.Linq;
using KernelManagementJam.Benchmarks;
using NUnit.Framework;
using Tests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class XorShiftRandomTests : NUnitTestsBase
    {
        [Test]
        public void SmokeTest1()
        {
            for (int l = 0; l <= 29; l++)
            {
                byte[] bytes1 = new byte[l];
                XorShiftRandom.FillByteArray(bytes1, 42);
                byte[] bytes2 = new byte[l];
                XorShiftRandom.FillByteArray(bytes2, 42);
                Console.WriteLine(AsString(bytes1) + " vs " + AsString(bytes2));
            }
        }

        static string AsString(byte[] bytes)
        {
            return string.Join("", bytes.Select(x => x.ToString("X2")));
        }

    }
}