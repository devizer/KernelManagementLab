using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.Benchmarks;
using Microsoft.Win32.SafeHandles;
using Universe.DiskBench;
using FileMode = System.IO.FileMode;

namespace Universe.Benchmark.DiskBench
{
    public interface IDiskBenchmark
    {
        DiskBenchmarkOptions Parameters { get; }
        ProgressInfo Progress { get; }
        void Perform();
    }
    
    public class DiskBenchmark : IDiskBenchmark
    {
        public DiskBenchmarkOptions Parameters { get; set; }
        public ProgressInfo Progress { get; private set; }
        
        public static readonly string BenchmarkTempFile = "bnchmrk.tmp";
        private string TempFile;
        
        private ProgressStep _allocate;
        private ProgressStep _seqRead;
        private ProgressStep _seqWrite;
        private ProgressStep _rndRead1T;
        private ProgressStep _rndWrite1T;
        private ProgressStep _rndReadN;
        private ProgressStep _rndWriteN;
        private ProgressStep _cleanUp;
        private ProgressStep _checkODirect;
        private bool _isODirectSupported;

        public DiskBenchmark(DiskBenchmarkOptions parameters)
        {
            Parameters = parameters;

            // copy/pasta from another ctor
            var workFolderFullName = new DirectoryInfo(Parameters.WorkFolder).FullName;
            TempFile = Path.Combine(workFolderFullName, BenchmarkTempFile);
            BuildProgress();
        }

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
                WorkingSetSize = fileSize,
                Flavour = flavour,
                RandomAccessBlockSize = randomAccessBlockSize,
                StepDuration = stepDuration,
                DisableODirect = disableODirect,
                ThreadsNumber = threadsNumber,
            };

            // copy/pasta from another ctor
            var workFolderFullName = new DirectoryInfo(Parameters.WorkFolder).FullName;
            TempFile = Path.Combine(workFolderFullName, BenchmarkTempFile);
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
            
            this.Progress = new ProgressInfo()
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
                Progress.IsCompleted = true;
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
                long maxIndex = Parameters.WorkingSetSize / Parameters.RandomAccessBlockSize;
                long pos = Parameters.RandomAccessBlockSize * (long) Math.Floor(random.NextDouble() * maxIndex);
                int count = (int) Math.Min(Parameters.WorkingSetSize - pos, Parameters.RandomAccessBlockSize);
                if (count != Parameters.RandomAccessBlockSize) return 0; // slip
                fs.Position = pos;
                return fs.Read(buffer, 0, count);
            };

            Func<Stream, byte[], int> doWrite = (fs, buffer) =>
            {
                long maxIndex = Parameters.WorkingSetSize / Parameters.RandomAccessBlockSize;
                long pos = Parameters.RandomAccessBlockSize * (long) Math.Floor(random.NextDouble() * maxIndex);
                int count = (int) Math.Min(Parameters.WorkingSetSize - pos, Parameters.RandomAccessBlockSize);
                fs.Position = pos;
                fs.Write(buffer, 0, count);
                return count;
            };

            Action<Exception> doCleanUp = (ex) =>
            {
                _cleanUp.Start();
                if (File.Exists(TempFile))
                    File.Delete(TempFile);

                if (ex != null) _cleanUp.Name = "Benchmark failed";
                _cleanUp.Complete();
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
                doCleanUp(null);
            }
            catch(Exception ex)
            {
                doCleanUp(ex);
                bool first = true;
                foreach (var step in Progress.Steps)
                {
                    if (step.State == ProgressStepState.InProgress || step.State == ProgressStepState.Pending)
                    {
                        step.State = first ? ProgressStepState.Error : ProgressStepState.Skipped;
                        first = false;
                    }
                }

                _cleanUp.State = ProgressStepState.Error;
                Console.WriteLine($"Benchmark for [{Parameters.WorkFolder}] failed. {ex.GetExceptionDigest()}{Environment.NewLine}{ex}");
                throw;
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

            _checkODirect.Name = _isODirectSupported ? "Direct Access is detected" : "Direct Access is absent";
            _checkODirect.Complete();
        }
        
        private void Allocate()
        {
            byte[] buffer = new byte[Math.Min(128 * 1024, this.Parameters.WorkingSetSize)];
            // new Random().NextBytes(buffer);
            new DataGenerator(Parameters.Flavour).NextBytes(buffer);
            using (FileStream fs = new FileStream(TempFile, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, FileOptions.WriteThrough))
            {
                _allocate.Start();
                long len = 0;
                while (len < this.Parameters.WorkingSetSize)
                {
                    var count = (int) Math.Min(this.Parameters.WorkingSetSize - len, buffer.Length);
                    fs.Write(buffer, 0, count);
                    len += count;
                    _allocate.Progress(len / (double) Parameters.WorkingSetSize, len);
                }
                _allocate.Complete();
            }
            LinuxKernelCacheFlusher.Sync();
        }
        
        private void SeqRead()
        {
            LinuxKernelCacheFlusher.Sync();
            byte[] buffer = new byte[Math.Min(1024 * 1024, this.Parameters.WorkingSetSize)];
            using (FileStream fs = new FileStream(TempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, buffer.Length))
            // using (FileStream fs = OpenFileStreamWithoutCacheOnLinux(buffer.Length))
            {
                _seqRead.Start();
                long len = 0;
                while (len < this.Parameters.WorkingSetSize)
                {
                    var count = (int) Math.Min(this.Parameters.WorkingSetSize - len, buffer.Length);
                    int n = fs.Read(buffer, 0, count);
                    len += n;
                    _seqRead.Progress(len / (double) Parameters.WorkingSetSize, len);
                    if (len >= Parameters.WorkingSetSize) fs.Position = 0;
                }
                _seqRead.Complete();
            }
        }

        private void SeqWrite()
        {
            LinuxKernelCacheFlusher.Sync();
            byte[] buffer = new byte[1024 * 1024];
            // new Random().NextBytes(buffer);
            new DataGenerator(Parameters.Flavour).NextBytes(buffer);
            using (FileStream fs = new FileStream(TempFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, buffer.Length, FileOptions.WriteThrough))
            {
                _seqWrite.Start();
                long len = 0;
                while (len < this.Parameters.WorkingSetSize)
                {
                    var count = (int)Math.Max(1, Math.Min(buffer.Length, this.Parameters.WorkingSetSize - buffer.Length));
                    fs.Write(buffer, 0, count);
                    len += count;
                    _seqWrite.Progress(len / (double) Parameters.WorkingSetSize, len);
                    if (len >= Parameters.WorkingSetSize) fs.Position = 0;
                }
                _seqWrite.Complete();
            }
        }

        private void RandomAccess(ProgressStep step, int numThreads, Func<Stream> getFileStream, Func<Stream,byte[],int> doStuff, long msecDuration)
        {
            LinuxKernelCacheFlusher.Sync();
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

    }
}