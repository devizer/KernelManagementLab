using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using KernelManagementJam.Benchmarks;
using Microsoft.Win32.SafeHandles;
using Universe.DiskBench;
using FileMode = System.IO.FileMode;

namespace Universe.Benchmark.DiskBench
{
    public class DiskBenchmark
    {
        public DiskBenchmarkOptions Parameters { get; set; }
        public ProgressInfo Prorgess { get; private set; }
        
        static readonly string TempName = "benchmark.tmp";
        private string TempFile;
        
        private ProgressStep _seqRead;
        private ProgressStep _seqWrite;
        private ProgressStep _rndRead1T;
        private ProgressStep _rndWrite1T;
        private ProgressStep _rndReadN;
        private ProgressStep _rndWriteN;
        private ProgressStep _cleanUp;
        private ProgressStep _allocate;
        private ProgressStep _checkODirect;
        private bool _isODirectSupported;

        public DiskBenchmark(
            string workFolder,
            long fileSize = 4L * 1024 * 1024 * 1024,
            DataGeneratorFlavour flavour = DataGeneratorFlavour.Random, 
            int randomAccessBlockSize = 4 * 1024,
            int stepDuration = 20000,
            bool disableODirect = false,
            int threadsNumber = 16
        )
        {
            Parameters = new DiskBenchmarkOptions()
            {
                WorkFolder = workFolder,
                FileSize = fileSize,
                Flavour = flavour,
                RandomAccessBlockSize = randomAccessBlockSize,
                StepDuration = stepDuration,
                DisableODirect = disableODirect,
                ThreadsNumber = threadsNumber,
            };

            var workFolderFullName = new DirectoryInfo(Parameters.WorkFolder).FullName;
            TempFile = Path.Combine(workFolderFullName, TempName);
            BuildProgress();
        }
        
        void BuildProgress()
        {
            if (Parameters.DisableODirect)
            {
                _checkODirect = new ProgressStep("Direct Access is disabled");
                _isODirectSupported = false;
                _checkODirect.Start();
                _checkODirect.Complete();
            }
            else
            {
                _checkODirect = new ProgressStep("Checking capabilities");
            }
                
            _allocate = new ProgressStep("Allocate");
            _seqRead = new ProgressStep("Sequential read");
            _seqWrite = new ProgressStep("Sequential write");
            _rndRead1T = new ProgressStep("Random Read, 1 thread");
            _rndWrite1T = new ProgressStep("Random Write, 1 thread");
            _rndReadN = new ProgressStep($"Random Read, {Parameters.ThreadsNumber} threads");
            _rndWriteN = new ProgressStep($"Random Write, {Parameters.ThreadsNumber} threads");
            _cleanUp = new ProgressStep("Clean up");
            
            this.Prorgess = new ProgressInfo()
            {
                Steps =
                {
                    _checkODirect,
                    _allocate,
                    _seqRead,
                    _seqWrite,
                    _rndRead1T,
                    _rndWrite1T,
                    _rndReadN,
                    _rndWriteN,
                    _cleanUp
                }
            };
        }


        public void Perform()
        {
            try
            {
                Perform_Impl();
            }
            finally
            {
                Prorgess.IsCompleted = true;
            }
        }

        private void Perform_Impl()
        {
            Random random = new Random();

            Func<FileStream> getFileWriter = () =>
            {
                return new FileStream(TempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,
                    this.Parameters.RandomAccessBlockSize, FileOptions.WriteThrough);
            };

            Func<Stream> getFileReader = () =>
            {
                // if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                //    return new LinuxDirectReadonlyFileStream(TempFile, RandomAccessBlockSize);

                if (!Parameters.DisableODirect && _isODirectSupported)
                    return new LinuxDirectReadonlyFileStreamV2(TempFile, this.Parameters.RandomAccessBlockSize);

                return new FileStream(TempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,
                    this.Parameters.RandomAccessBlockSize, FileOptions.WriteThrough);
            };

            Func<Stream, byte[], int> doRead = (fs, buffer) =>
            {
                long maxIndex = Parameters.FileSize / Parameters.RandomAccessBlockSize;
                long pos = Parameters.RandomAccessBlockSize * (long) Math.Floor(random.NextDouble() * maxIndex);
                int count = (int) Math.Min(Parameters.FileSize - pos, Parameters.RandomAccessBlockSize);
                if (count != Parameters.RandomAccessBlockSize) return 0; // slip
                fs.Position = pos;
                return fs.Read(buffer, 0, count);
            };

            Func<Stream, byte[], int> doWrite = (fs, buffer) =>
            {
                long maxIndex = Parameters.FileSize / Parameters.RandomAccessBlockSize;
                long pos = Parameters.RandomAccessBlockSize * (long) Math.Floor(random.NextDouble() * maxIndex);
                int count = (int) Math.Min(Parameters.FileSize - pos, Parameters.RandomAccessBlockSize);
                fs.Position = pos;
                fs.Write(buffer, 0, count);
                return count;
            };

            try
            {
                if (!Parameters.DisableODirect) CheckODirect();
                Allocate();
                SeqRead();
                SeqWrite();
                RandomAccess(_rndRead1T, 1, getFileReader, doRead, Parameters.StepDuration);
                RandomAccess(_rndWrite1T, 1, getFileWriter, doWrite, Parameters.StepDuration);
                RandomAccess(_rndReadN, Parameters.ThreadsNumber, getFileReader, doRead, Parameters.StepDuration);
                RandomAccess(_rndWriteN, Parameters.ThreadsNumber, getFileWriter, doWrite, Parameters.StepDuration);
            }
            finally
            {
                _cleanUp.Start();
                if (File.Exists(TempFile))
                    File.Delete(TempFile);

                _cleanUp.Complete();
            }
        }

        private void CheckODirect()
        {
            _checkODirect.Start();

            _isODirectSupported = false;
            try
            {
                _isODirectSupported = ODirectCheck.IsO_DirectSupported(Parameters.WorkFolder, 128 * 1024);
            }
            catch
            {
            }

            _checkODirect.Name = _isODirectSupported ? "Direct Access is present" : "Direct Access is absent";
            _checkODirect.Complete();
        }
        
        private void Allocate()
        {
            byte[] buffer = new byte[Math.Min(128 * 1024, this.Parameters.FileSize)];
            // new Random().NextBytes(buffer);
            new DataGenerator(Parameters.Flavour).NextBytes(buffer);
            using (FileStream fs = new FileStream(TempFile, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, FileOptions.WriteThrough))
            {
                _allocate.Start();
                long len = 0;
                while (len < this.Parameters.FileSize)
                {
                    var count = (int) Math.Min(this.Parameters.FileSize - len, buffer.Length);
                    fs.Write(buffer, 0, count);
                    len += count;
                    _allocate.Progress(len / (double) Parameters.FileSize, len);
                }
                _allocate.Complete();
            }
            Sync();
        }
        
        private void SeqRead()
        {
            Sync();
            byte[] buffer = new byte[Math.Min(1024 * 1024, this.Parameters.FileSize)];
            using (FileStream fs = new FileStream(TempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, buffer.Length))
            // using (FileStream fs = OpenFileStreamWithoutCacheOnLinux(buffer.Length))
            {
                _seqRead.Start();
                long len = 0;
                while (len < this.Parameters.FileSize)
                {
                    var count = (int) Math.Min(this.Parameters.FileSize - len, buffer.Length);
                    int n = fs.Read(buffer, 0, count);
                    len += n;
                    _seqRead.Progress(len / (double) Parameters.FileSize, len);
                    if (len >= Parameters.FileSize) fs.Position = 0;
                }
                _seqRead.Complete();
            }
        }

        private void SeqWrite()
        {
            Sync();
            byte[] buffer = new byte[1024 * 1024];
            // new Random().NextBytes(buffer);
            new DataGenerator(Parameters.Flavour).NextBytes(buffer);
            using (FileStream fs = new FileStream(TempFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, buffer.Length, FileOptions.WriteThrough))
            {
                _seqWrite.Start();
                long len = 0;
                while (len < this.Parameters.FileSize)
                {
                    var count = (int)Math.Max(1, Math.Min(buffer.Length, this.Parameters.FileSize - buffer.Length));
                    fs.Write(buffer, 0, count);
                    len += count;
                    _seqWrite.Progress(len / (double) Parameters.FileSize, len);
                    if (len >= Parameters.FileSize) fs.Position = 0;
                }
                _seqWrite.Complete();
            }
        }

        private void RandomAccess(ProgressStep step, int numThreads, Func<Stream> getFileStream, Func<Stream,byte[],int> doStuff, long msecDuration)
        {
            Sync();
            List<Thread> threads = new List<Thread>();
            CountdownEvent started = new CountdownEvent(numThreads);
            CountdownEvent finished = new CountdownEvent(numThreads);
            Stopwatch sw = null;
            object syncStopwatch = new object();
            Func<Stopwatch> getStopwatch = () =>
            {
                if (sw != null) return sw;
                lock (syncStopwatch)
                {
                    if (sw == null) sw = Stopwatch.StartNew();
                    return sw;
                }
            };
            long totalSize = 0;
            long prevNotification = 0;
            step.Start();
            ConcurrentBag<Exception> errors = new ConcurrentBag<Exception>();
            
            for (int t = 0; t < numThreads; t++)
            {
                Thread thread = new Thread(_ =>
                {
                    try
                    {
                        using (Stream fs = getFileStream())
                        {
                            byte[] buffer = new byte[Parameters.RandomAccessBlockSize];
                            // new Random().NextBytes(buffer);
                            new DataGenerator(Parameters.Flavour).NextBytes(buffer);
                            started.Signal();
                            started.Wait();
                            Stopwatch stopwatch = getStopwatch();
                            do
                            {
                                int increment = doStuff(fs, buffer);
                                var total = Interlocked.Add(ref totalSize, increment);

                                long currentStopwatch = stopwatch.ElapsedMilliseconds;
                                step.Progress(currentStopwatch / (double) msecDuration, total);

                            } while (stopwatch.ElapsedMilliseconds <= msecDuration);
                        }
                    }
                    catch (Exception ex)
                    {
                        // started.Signal();
                        errors.Add(ex);
                    }

                    finished.Signal();
                }) { IsBackground = true};
                thread.Start();
                threads.Add(thread);
            }

            started.Wait();
            finished.Wait();
            step.Complete();

            foreach (var thread in threads)
                thread.Join();

            if (!errors.IsEmpty)
            {
                throw new AggregateException($"At least single thread failed, for example {errors.First().Message}", errors);
            }
        }

//        static FileOptions GetReadOptions()
//        {
//            const int O_DIRECT = 0x4000;
//            const int FILE_FLAG_NO_BUFFERING = 0x20000000;
//
//            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
//                return (FileOptions) FILE_FLAG_NO_BUFFERING;
//            else
//                return (FileOptions) O_DIRECT;
//            
//        }

        FileStream OpenFileStreamWithoutCacheOnLinux_Legacy(int bufferSize)
        {
            var openFlags = Mono.Unix.Native.OpenFlags.O_LARGEFILE | Mono.Unix.Native.OpenFlags.O_SYNC | Mono.Unix.Native.OpenFlags.O_DIRECT |  Mono.Unix.Native.OpenFlags.O_RDONLY;
            int handle = Mono.Unix.Native.Syscall.open(TempFile, openFlags);
            IntPtr rawHandle = new IntPtr(handle);
            SafeFileHandle fh = new SafeFileHandle(rawHandle, false);
            FileStream fs = new FileStream(fh, FileAccess.ReadWrite, bufferSize, false);
            
            return fs;
        }

        public static void Sync()
        {
            SyncWriteBuffer();
            FlushReadBuffers();
        }

        public static void SyncWriteBuffer()
        {

            StartAndIgnore("sync", "");
        }

        public static void FlushReadBuffers()
        {
            bool isDropOk = false;
            try
            {
                File.WriteAllText("/proc/sys/vm/drop_caches", "1");
                isDropOk = true;
            }
            catch
            {
            }

            if (!isDropOk)
            {
                StartAndIgnore("sudo", "sh -c \"echo 1 > /proc/sys/vm/drop_caches\"");
            }
        }
        
        static int StartAndIgnore(string fileName, string args)
        {
            Stopwatch sw = Stopwatch.StartNew(); 
            string info = fileName == "sudo" ? args : fileName;
                
            try
            {
                ProcessStartInfo si = new ProcessStartInfo(fileName, args);
                si.RedirectStandardError = true;
                si.RedirectStandardOutput = true;
                using (Process p = Process.Start(si))
                {
                    p.Start();
                    p.WaitForExit();
#if DEBUG
                    Console.WriteLine($"Process [{info}] successfully finished in {sw.ElapsedMilliseconds:n0} milliseconds");
#endif
                    return p.ExitCode;
                }
            }
            catch
            {
#if DEBUG
                Console.WriteLine($"Process [{info}] failed in {sw.ElapsedMilliseconds:n0} milliseconds");
#endif
                return -1;
            }
        }
    }
}