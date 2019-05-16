using System;
using System.IO;
using Mono.Unix;

namespace KernelManagementJam.Benchmarks
{
    public class LinuxDirectReadonlyFileStream : Stream
    {
        private long _Position = 0;
        public readonly string FileName;
        public readonly int BlockSize;
        private UnixStream _back;
        private int _fileDescriptor;

        public LinuxDirectReadonlyFileStream(string fileName, int blockSize)
        {
            FileName = fileName;
            BlockSize = blockSize;
            
            var openFlags = /* Mono.Unix.Native.OpenFlags.O_SYNC |  Mono.Unix.Native.OpenFlags.O_DIRECT | */ Mono.Unix.Native.OpenFlags.O_RDONLY;
            _fileDescriptor = Mono.Unix.Native.Syscall.open(FileName, openFlags);
            _back = new UnixStream(_fileDescriptor, true);
            _back.AdviseFileAccessPattern(FileAccessPattern.NoReuse);
        }

        public override void Flush()
        {
            // it is a readnly - nothing to do
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count != BlockSize)
                throw new ArgumentException($"count arg should be equal to block size {BlockSize}");

            // Console.WriteLine($"Reading {count} bytes. Length is {Length}");
            // long prevPos = Position;
            var ret = _back.Read(buffer, offset, count);
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