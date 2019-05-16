using System;
using System.IO;
using System.Net.Sockets;
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
            
            var openFlags = /* Mono.Unix.Native.OpenFlags.O_SYNC |  Mono.Unix.Native.OpenFlags.O_DIRECT | */ Mono.Unix.Native.OpenFlags.O_RDONLY;
            _fileDescriptor = Mono.Unix.Native.Syscall.open(FileName, openFlags);
            bool isError = 0 != Mono.Unix.Native.Syscall.posix_fadvise(_fileDescriptor, 0, 0, PosixFadviseAdvice.POSIX_FADV_NOREUSE | PosixFadviseAdvice.POSIX_FADV_DONTNEED);
            _back = new UnixStream(_fileDescriptor, true);
            _back.AdviseFileAccessPattern(FileAccessPattern.NoReuse | FileAccessPattern.Random | FileAccessPattern.FlushCache);
            
            TheBuffer = new byte[BlockSize*2];
        }

        unsafe void AlignedInterop(Action<IntPtr> action)
        {
            fixed (byte* start = &TheBuffer[0])
            {
                IntPtr ptrStart = (IntPtr) start;
                long addrStart = ptrStart.ToInt64();
                long addrBuffer = addrStart;
                long delta = addrBuffer % AlignmentScale;
                if (delta != 0)
                {
                    addrBuffer += -delta + AlignmentScale;
                }

                var alignedPointer = new IntPtr(addrBuffer);
                
                WriteLine($"addrBuffer: {addrBuffer} alignedPointer: {alignedPointer}, TheBuffer pointer: {ptrStart}");

                action(alignedPointer);
            }
        }

        public override void Flush()
        {
            // it is a readonly - nothing to do
        }

        static void WriteLine(string s)
        {
            // Console.WriteLine(s);
        }

        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            if (count != BlockSize)
                throw new ArgumentException($"count arg should be equal to block size {BlockSize}");

            int ret = -1;
            WriteLine($"Reading {count} bytes. file description is [{_fileDescriptor}]");
            
            AlignedInterop(alignedPointer =>
            {
                // http://man7.org/linux/man-pages/man2/read.2.html
                long num;
                Errno errno = Stdlib.GetLastError();
                do
                {
                    num = Syscall.read(_fileDescriptor, alignedPointer, (ulong)count);
                    errno = Stdlib.GetLastError();
                    WriteLine($"syscall.read returns {num}, error: {errno}");
                }
                while (false && UnixMarshal.ShouldRetrySyscall((int) num));
                
                if (num < 0)
                    throw new InvalidOperationException($"Syscall error {errno}");

                ret = (int) num;

                fixed (byte* output = &buffer[0])
                {
                    byte* fileBuffer = (byte*) alignedPointer;
                    byte* dst = output;
                    for (int i = 0; i < num; i++)
                    {
                        *dst++ = *fileBuffer++;
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
            throw new System.NotSupportedException();
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
            get
            {
                return false;
            }
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
                // Console.WriteLine($"Set Position to {value}");
            }
        }
        
        
    }
}