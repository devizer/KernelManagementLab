using System;
using System.IO;
using System.Runtime.InteropServices;
using KernelManagementJam.Benchmarks;
using NUnit.Framework;
using Universe.NUnitTests;
using Universe;
using Universe.Benchmark.DiskBench;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class DiskBenchmarkTests : NUnitTestsBase
    {
        // HDD is preferred
        private string CurrentDirectory = "/hdd";
        private readonly int FileSize = 4 * 1024 * 1024; //Kb
        private readonly int BlockSize = 64 * 1024;

        [SetUp] public void Setup()
        {
            if (!Directory.Exists(CurrentDirectory))
                CurrentDirectory = new DirectoryInfo(Environment.CurrentDirectory).FullName;
        }

        [Test]
        public void _0_Platform()
        {
            Console.WriteLine($"RuntimeInformation.OSArchitecture: {RuntimeInformation.OSArchitecture}");
        }

        [Test]
        public void _1_DiskBenchmark()
        {
            var tempSize = 128*1024;
            DiskBenchmark jit = new DiskBenchmark(".", tempSize, DataGeneratorFlavour.Random, tempSize, 1);
            jit.Perform();
        }

        [Test]
        public void _2_O_Direct_Half()
        {
            if (HugeCrossInfo.ThePlatform == HugeCrossInfo.Platform.Windows)
            {
                Console.WriteLine("LinuxDirectReadonlyFileStreamV2 needs Linux or MacOS");
                return;
            }
            
            Environment.CurrentDirectory = CurrentDirectory;
            var size = FileSize;
            var block = BlockSize;
            var oDirectFile = "O_Direct.file";
            byte[] original = new byte[size];
            using (FileStream fs = new FileStream(oDirectFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                new Random(1).NextBytes(original);
                fs.Write(original,0, original.Length);
            }

            MemoryStream readerCopy = new MemoryStream();
            using (LinuxDirectReadonlyFileStreamV2 stream = new LinuxDirectReadonlyFileStreamV2(oDirectFile, block))
            {
                byte[] tmp = new byte[block];
                for (int i = 0; i < size / block; i++)
                {
                    // Console.WriteLine($"On Reading {i+1}");
                    int n = stream.Read(tmp, 0, tmp.Length);
                    Console.WriteLine($"{i+1} / {size / block}: read {n} bytes");
                    readerCopy.Write(tmp, 0, n);
                }
            }
            
            CollectionAssert.AreEqual(original, readerCopy.ToArray());

            CleanUp(oDirectFile);
        }

        private static void CleanUp(string oDirectFile)
        {
            try
            {
                File.Delete(oDirectFile);
            }
            catch
            {
            }
        }

        [Test]
        public void _3_O_Direct()
        {
            if (HugeCrossInfo.ThePlatform == HugeCrossInfo.Platform.Windows)
            {
                Console.WriteLine("LinuxDirectReadonlyFileStreamV2 needs Linux or MacOS");
                return;
            }

            Environment.CurrentDirectory = CurrentDirectory;
            var oDirectFile = "O_Direct.file";
            byte[] original = new byte[FileSize];
            using (FileStream fs = new FileStream(oDirectFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                new Random(1).NextBytes(original);
                fs.Write(original,0, original.Length);
            }

            MemoryStream readerCopy = new MemoryStream();
            using (LinuxDirectReadonlyFileStreamV2 stream = new LinuxDirectReadonlyFileStreamV2(oDirectFile, BlockSize))
            {
                byte[] tmp = new byte[BlockSize];
                int i = 0;
                while(true)
                {
                    // Console.WriteLine($"On Reading {i+1}");
                    int n = stream.Read(tmp, 0, tmp.Length);
                    Console.WriteLine($"{i+1} / {FileSize / BlockSize}: read {n} bytes");
                    if (n <= 0) break;
                    readerCopy.Write(tmp, 0, n);
                    i++;
                }
            }
            
            CollectionAssert.AreEqual(original, readerCopy.ToArray());

            CleanUp(oDirectFile);
        }

        [Test]
        public void _4_O_DirectCheck_OfCurrent()
        {
            string dir = new DirectoryInfo(".").FullName;
            Console.WriteLine($"Checking O_DIRECT support for {dir}");
            Console.WriteLine($"O_DIRECT is supported:  {DiskBenchmarkChecks.IsO_DirectSupported(dir)}");
        }

        [Test]
        public void _4_O_DirectCheck_OfTmp()
        {
            string dir = "/tmp";
            if (!Directory.Exists(dir)) return;
            Console.WriteLine($"Checking O_DIRECT support for {dir}");
            Console.WriteLine($"O_DIRECT is supported:  {DiskBenchmarkChecks.IsO_DirectSupported(dir)}");
        }

        [Test]
        public void _4_O_DirectCheck_Another()
        {
            string dir = "/transient-builds";
            if (!Directory.Exists(dir)) return;
            Console.WriteLine($"Checking O_DIRECT support for {dir}");
            Console.WriteLine($"O_DIRECT is supported:  {DiskBenchmarkChecks.IsO_DirectSupported(dir)}");
        }
    }
    
}
