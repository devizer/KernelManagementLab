using System;
using System.IO;

namespace KernelManagementJam.Benchmarks
{
    public class ODirectCheck
    {
        private static readonly int DefaultGranularity = 128 * 1024;

        public static bool IsO_DirectSupported(string directory)
        {
            return IsO_DirectSupported(directory, DefaultGranularity);
        }

        public static bool IsO_DirectSupported(string directory, int granularity)
        {
            if (granularity % 512 != 0)
                throw new ArgumentException("granularity argument should be multiplier of 512", nameof(granularity));
                
            string fileName = Path.Combine(
                new DirectoryInfo(directory).FullName,
                $"o-direct-{Guid.NewGuid():N}");

            byte[] original = new byte[granularity];
            new Random(42).NextBytes(original);
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, granularity))
            {
                fs.Write(original, 0, original.Length);
            }

            bool ret;
            try
            {
                using (var stream = new LinuxDirectReadonlyFileStreamV2(fileName, granularity))
                {
                    MemoryStream mem = new MemoryStream(granularity);
                    stream.CopyTo(mem);
                    var copy = mem.ToArray();
                    ret = AreEquals(original, copy);
                }
            }
            catch(Exception ex)
            {
                ret = false;
#if DEBUG
                Console.Write($"O_DIRECT check is negative for {Path.GetDirectoryName(fileName)}. {ex}");
#endif
            }

            try
            {
                File.Delete(fileName);
            }
            catch
            {
            }

            return ret;
        }

        private static unsafe bool AreEquals(byte[] arrayOne, byte[] arrayTwo)
        {
            if (arrayOne.Length != arrayTwo.Length) return false;
            int n = arrayOne.Length / 8;

            fixed (byte* one = &arrayOne[0])
            fixed (byte* two = &arrayTwo[0])
            {
                long* ptr1 = (long*) one;
                long* ptr2 = (long*) two;
                while (n > 0)
                {
                    if (*ptr1 != *ptr2) return false;
                    ptr1++;
                    ptr2++;
                    n--;
                }
            }

            return true;
        }
    }
}