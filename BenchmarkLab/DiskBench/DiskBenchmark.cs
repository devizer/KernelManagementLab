using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Universe.DiskBench;

namespace Universe.Benchmark.DiskBench
{
    public class DiskBenchmark
    {
        public ProgressInfo Prorgess { get; private set; }
        private string WorkFolder { get; }
        private long FileSize { get; }
        private int RandomAccessBlockSize { get; }
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

        public DiskBenchmark(string workFolder, long fileSize, int randomAccessBlockSize)
        {
            WorkFolder = workFolder;
            FileSize = fileSize;
            TempFile = Path.Combine(new DirectoryInfo(WorkFolder).FullName, TempName);
            RandomAccessBlockSize = randomAccessBlockSize;
        }

        public void Perform()
        {
            Random random = new Random();
            
            Func<FileStream> getFile = () =>
            {
                return new FileStream(TempName, FileMode.Open, FileAccess.ReadWrite, FileShare.None,
                    this.RandomAccessBlockSize, FileOptions.WriteThrough);
            };

            Func<FileStream, byte[], int> doRead = (fs, buffer) =>
            {
                long maxIndex = FileSize / RandomAccessBlockSize;
                long pos = RandomAccessBlockSize * (long) Math.Floor(random.NextDouble() * maxIndex);
                int count = (int) Math.Min(FileSize - pos, RandomAccessBlockSize);
                fs.Position = pos;
                return fs.Read(buffer, 0, count);
            };

            Func<FileStream, byte[], int> doWrite = (fs, buffer) =>
            {
                long maxIndex = FileSize / RandomAccessBlockSize;
                long pos = RandomAccessBlockSize * (long) Math.Floor(random.NextDouble() * maxIndex);
                int count = (int) Math.Min(FileSize - pos, RandomAccessBlockSize);
                fs.Position = pos;
                fs.Write(buffer, 0, count);
                return count;
            };
            
            const int seconds = 30;
            try
            {
                Allocate();
                SeqRead();
                SeqWrite();
                RandomAccess(_rndRead1T, 1, getFile, doRead, seconds*1000);
                RandomAccess(_rndWrite1T, 1, getFile, doWrite, seconds*1000);
                RandomAccess(_rndReadN, 16, getFile, doRead, seconds*1000);
                RandomAccess(_rndWriteN, 16, getFile, doWrite, seconds*1000);
            }
            finally
            {
                _cleanUp.Start();
                if (File.Exists(TempFile))
                    File.Delete(TempFile);
                
                _cleanUp.Complete();
            }
        }

        void BuildProgress()
        {

            _allocate = new ProgressStep("Allocate");
            _seqRead = new ProgressStep("Sequential read");
            _seqWrite = new ProgressStep("Sequential write");
            _rndRead1T = new ProgressStep("Random Read, 1 thread");
            _rndWrite1T = new ProgressStep("Random Write, 1 thread");
            _rndReadN = new ProgressStep("Random Read, 16 threads");
            _rndWriteN = new ProgressStep("Random Write, 16 threads");
            _cleanUp = new ProgressStep("Clean up");
            
            this.Prorgess = new ProgressInfo()
            {
                Steps =
                {
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

        private void Allocate()
        {
            byte[] buffer = new byte[128 * 1024];
            using (FileStream fs = new FileStream(TempFile, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, FileOptions.WriteThrough))
            {
                _allocate.Start();
                long len = 0;
                while (len < this.FileSize)
                {
                    var count = (int) Math.Min(this.FileSize - len, buffer.Length);
                    fs.Write(buffer, 0, count);
                    len += count;
                    _allocate.Progress(len / (double) FileSize, len);
                }
                _allocate.Complete();
            }
        }
        
        private void SeqRead()
        {
            byte[] buffer = new byte[1024 * 1024];
            using (FileStream fs = new FileStream(TempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, buffer.Length))
            {
                _seqRead.Start();
                long len = 0;
                while (len < this.FileSize)
                {
                    var count = (int) Math.Min(this.FileSize - len, buffer.Length);
                    int n = fs.Read(buffer, 0, count);
                    len += n;
                    _seqRead.Progress(len / (double) FileSize, len);
                    if (len >= FileSize) fs.Position = 0;
                }
                _seqRead.Complete();
            }
        }

        private void SeqWrite()
        {
            byte[] buffer = new byte[1024 * 1024];
            using (FileStream fs = new FileStream(TempFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, buffer.Length, FileOptions.WriteThrough))
            {
                _seqWrite.Start();
                long len = 0;
                while (len < this.FileSize - buffer.Length)
                {
                    fs.Write(buffer, 0, buffer.Length);
                    len += buffer.Length;
                    _seqWrite.Progress(len / (double) FileSize, len);
                    if (len >= FileSize) fs.Position = 0;
                }
                _seqWrite.Complete();
            }
        }

        private void RandomAccess(ProgressStep step, int numThreads, Func<FileStream> getFileStream, Func<FileStream,byte[],int> doStuff, long msecDuration)
        {
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
                        using (FileStream fs = getFileStream())
                        {
                            byte[] buffer = new byte[RandomAccessBlockSize];
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
                        started.Signal();
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

    }
}