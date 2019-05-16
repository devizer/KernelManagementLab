using System;
using System.IO;
using System.Runtime.InteropServices;
using KernelManagementJam.Benchmarks;
using NUnit.Framework;
using Universe.Benchmark.DiskBench;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class IntegrationTests
    {

        [Test]
        public void _0_Platform()
        {
            Console.WriteLine($"RuntimeInformation.OSArchitecture: {RuntimeInformation.OSArchitecture}");
        }

        [Test]
        public void _1_DiskBenchmark()
        {
            var tempSize = 128*1024;
            DiskBenchmark jit = new DiskBenchmark(".", tempSize, tempSize, 1);
            jit.Perform();
        }

        [Test]
        public void _2_O_Direct_Half()
        {
            Environment.CurrentDirectory = "/hdd";
            var size = 1024 * 1024;
            var block = 16 * 1024;
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
                    Console.WriteLine($"On Reading {i+1}");
                    int n = stream.Read(tmp, 0, tmp.Length);
                    Console.WriteLine($"{i+1} / {size / block}: read {n} bytes");
                    readerCopy.Write(tmp, 0, n);
                }
            }
            
            CollectionAssert.AreEqual(original, readerCopy.ToArray());

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
            Environment.CurrentDirectory = "/hdd";
            var size = 1024 * 1024;
            var block = 16 * 1024;
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
                int i = 0;
                while(true)
                {
                    // Console.WriteLine($"On Reading {i+1}");
                    int n = stream.Read(tmp, 0, tmp.Length);
                    Console.WriteLine($"{i+1} / {size / block}: read {n} bytes");
                    if (n <= 0) break;
                    readerCopy.Write(tmp, 0, n);
                    i++;
                }
            }
            
            CollectionAssert.AreEqual(original, readerCopy.ToArray());

            try
            {
                File.Delete(oDirectFile);
            }
            catch
            {
            }
        }
        
    }
}