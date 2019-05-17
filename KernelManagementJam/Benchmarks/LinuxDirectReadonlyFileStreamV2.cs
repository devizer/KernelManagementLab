using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Mono.Unix;
using Mono.Unix.Native;

namespace KernelManagementJam.Benchmarks
{
    public class LinuxDirectReadonlyFileStreamV2 : Stream
    {
        private long _Position = 0;
        public readonly string FileName;
        public readonly int BlockSize;
        private UnixStream _back;
        private int _fileDescriptor;

        private const int AlignmentScale = 16384;
        private byte[] TheBuffer;

        public LinuxDirectReadonlyFileStreamV2(string fileName, int blockSize)
        {
            FileName = fileName;
            BlockSize = blockSize;

            var openFlags = OpenFlags.O_SYNC | Mono.Unix.Native.OpenFlags.O_DIRECT |
                            Mono.Unix.Native.OpenFlags.O_RDONLY;
            _fileDescriptor = Syscall.open(FileName, openFlags);
            // for x390 a value of POSIX_FADV_NOREUSE is 7 ?:::?
            bool isError = 0 != Syscall.posix_fadvise(_fileDescriptor, 0, 0,
                               PosixFadviseAdvice.POSIX_FADV_NOREUSE | PosixFadviseAdvice.POSIX_FADV_DONTNEED);
            if (isError)
                DebugLog(
                    $"POSIX_FADV_NOREUSE + POSIX_FADV_DONTNEED is not supported for the {Path.GetDirectoryName(new FileInfo(fileName).FullName)} folder");

            _back = new UnixStream(_fileDescriptor, true);
            _back.AdviseFileAccessPattern(FileAccessPattern.NoReuse | FileAccessPattern.Random |
                                          FileAccessPattern.FlushCache);

            TheBuffer = new byte[BlockSize * 2];
        }

        unsafe void AlignedInterop(Action<IntPtr> action)
        {
            fixed (byte* start = &TheBuffer[0])
            {
                IntPtr ptrStart = (IntPtr) start;
                long addrStart = ptrStart.ToInt64();
                long addrBuffer = addrStart;
                long delta = addrBuffer % this.BlockSize;
                if (delta != 0)
                {
                    addrBuffer += -delta + this.BlockSize;
                }

                var alignedPointer = new IntPtr(addrBuffer);

                DebugLog($"addrBuffer: {addrBuffer} alignedPointer: {alignedPointer}, TheBuffer pointer: {ptrStart}");

                action(alignedPointer);
            }
        }

        public override void Flush()
        {
            // it is a readonly - nothing to do
        }

        [Conditional("WTH")]
        static void DebugLog(string s)
        {
            Console.WriteLine(s);
        }

        public override unsafe int Read(byte[] buffer, int offset, int count)
        {
            if (count != BlockSize)
                throw new ArgumentException($"count arg should be equal to block size {BlockSize}");

            int ret = -1;
            DebugLog($"Reading {count} bytes. file description is [{_fileDescriptor}]");

            AlignedInterop(alignedPointer =>
            {
                // http://man7.org/linux/man-pages/man2/read.2.html
                long num;
                Errno errno;
                bool needRetry;
                do
                {
                    num = Syscall.read(_fileDescriptor, alignedPointer, (ulong) count);
                    errno = Stdlib.GetLastError();
                    DebugLog($"syscall.read returns {num}, error: {errno}");
                    needRetry = num == -1 && errno == Errno.EINTR;
                } while (needRetry);

                if (num < 0)
                    throw new InvalidOperationException($"Syscall error {errno}");

                ret = (int) num;

                fixed (byte* output = &buffer[0])
                {
                    byte* src = (byte*) alignedPointer;
                    byte* dst = output;
                    while (num >= 8)
                    {
                        *(long*) dst = *(long*) src;
                        dst += 8;
                        src += 8;
                        num -= 8;
                    }

                    while (num > 0)
                    {
                        *dst++ = *src++;
                        num--;
                    }
                }

            });
            // Console.WriteLine($"Reading {count} bytes. Length is {Length}");
            // long prevPos = Position;
            // long nextPos = Position;
            // Console.WriteLine($"Read {count} bytes. Pos changed {prevPos} --> {nextPos}");
            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _back.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new System.NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _back.Length; }
        }

        public override long Position
        {
            get { return _back.Position; }
            set
            {
                if (value % BlockSize != 0)
                    throw new ArgumentException($"Position value should be a multiplier of the block size {BlockSize}");

                _back.Position = value;
                DebugLog($"Set Position to {value}");
            }
        }


        public override void Close()
        {
            _back.Dispose();
            Console.WriteLine($"Disposed: {FileName} #{_fileDescriptor}");
        }
    }


    public class ODirectCheck
    {
        public static bool IsO_DirectSupported(string directory, int granularity)
        {
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
                Console.Write($"O_DIRECT check failed. {ex}");
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

        public static bool IsO_DirectSupported(string directory)
        {
            return IsO_DirectSupported(directory, 128 * 1024);
        }

        private unsafe static bool AreEquals(byte[] arrayOne, byte[] arrayTwo)
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

