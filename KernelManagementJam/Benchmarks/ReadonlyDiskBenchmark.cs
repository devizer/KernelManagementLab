using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.Benchmarks;
using Universe.DiskBench;

namespace Universe.Benchmark.DiskBench
{
    public class ReadonlyDiskBenchmark : IDiskBenchmark
    {
        // Files from another volumes should be used for readonly disk benchmark
        // It is used to stay in a Parameters.WorkFolder volume
        public readonly List<DriveDetails> Mounts;
        public DiskBenchmarkOptions Parameters { get; set; }
        public bool IsJit = false;
        
        private const int UnconditionalThreshold = 16384;
        private FileInfo[] WorkingSet = null;
        
        [Obsolete("TODO", true)] private string[] NormalizedMountPaths;
        
        public ProgressInfo Progress { get; private set; }
        
        private ProgressStep _analyze;
        private ProgressStep _checkODirect;
        private ProgressStep _seqRead;
        private ProgressStep _rndRead1T;
        private ProgressStep _rndReadN;
        private bool _isODirectSupported;

        public ReadonlyDiskBenchmark(DiskBenchmarkOptions parameters, List<DriveDetails> mounts = null)
        {
            Parameters = parameters;
            Mounts = mounts;
            
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
                _checkODirect = new ProgressStep("Check capabilities");
            }
                
            _analyze = new ProgressStep("Analyze metadata");
            _seqRead = new ProgressStep("Sequential read");
            _rndRead1T = new ProgressStep("Random Read, 1 thread");
            _rndReadN = new ProgressStep($"Random Read, {Parameters.ThreadsNumber} threads");
            
            this.Progress = new ProgressInfo()
            {
                Steps =
                {
                    _analyze,
                    _checkODirect,
                    _seqRead,
                    _rndRead1T,
                    _rndReadN,
                }
            };
        }
        
        public void Perform()
        {
            try
            {
                Perform_Impl();
            }
            catch (Exception ex)
            {
                // In case of fail the first pending step is replaced by ERROR status and the rest are SKIPPED
                bool first = true;
                foreach (var step in Progress.Steps)
                {
                    if (step.State == ProgressStepState.InProgress || step.State == ProgressStepState.Pending)
                    {
                        step.State = first ? ProgressStepState.Error : ProgressStepState.Skipped;
                        first = false;
                    }
                }

                ProgressStep failedStep = new ProgressStep($"Benchmark failed. {ex.GetExceptionDigest()}") {State = ProgressStepState.Error};
                Progress.Steps.Add(failedStep);
                

            }
            finally
            {
                Progress.IsCompleted = true;
            }
        }

        private void Perform_Impl()
        {
            Random random = new Random();
            AnalyzeMetadata();
            if (!Parameters.DisableODirect) CheckODirect();
            SeqRead();

            try
            {
                long totalSize = 0;
                foreach (var fileInfo in WorkingSet)
                {
                    fileInfo.Stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read,
                        Parameters.RandomAccessBlockSize);
                    
                    totalSize += fileInfo.Size;
                    if (totalSize > Parameters.WorkingSetSize) break;
                }

                FileInfo[] filteredFiles = WorkingSet.Where(x => x.Stream != null).ToArray();

                Func<byte[], int> doRead = (buffer) =>
                {
                    CancelIfRequested();
                    int indexFile = (int) Math.Floor(random.NextDouble() * filteredFiles.Length);
                    FileInfo fileInfo = filteredFiles[indexFile];
                    long maxIndex = fileInfo.Size / Parameters.RandomAccessBlockSize;
                    long pos = Parameters.RandomAccessBlockSize * (long) Math.Floor(random.NextDouble() * maxIndex);
                    int count = (int) Math.Min(fileInfo.Size - pos, Parameters.RandomAccessBlockSize);
                    if (count != Parameters.RandomAccessBlockSize) return 0; // slip
                    fileInfo.Stream.Position = pos;
                    return fileInfo.Stream.Read(buffer, 0, count);
                };

                RandomRead(_rndRead1T, 1, doRead, Parameters.StepDuration);
                RandomRead(_rndReadN, Parameters.ThreadsNumber, doRead, Parameters.StepDuration);
            }
            finally
            {
                foreach (var fileInfo in WorkingSet)
                {
                    fileInfo.Stream?.Close();
                }
            }
        }
        
        public bool IsCanceled { get; private set; }
        public void RequestCancel()
        {
            IsCanceled = true;
        }
        
        private void CancelIfRequested()
        {
            if (IsCanceled)
                throw new BenchmarkCanceledException($"Disk benchmark for {Parameters.WorkFolder} canceled"); 
        }



        private void RandomRead(ProgressStep step, int numThreads, Func<byte[], int> doStuff, int msecDuration)
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
                        byte[] buffer = new byte[Parameters.RandomAccessBlockSize];
                        started.Signal();
                        started.Wait();
                        Stopwatch stopwatch = getStopwatch();
                        do
                        {
                            int increment = doStuff(buffer);
                            var total = Interlocked.Add(ref totalSize, increment);

                            long currentStopwatch = stopwatch.ElapsedMilliseconds;
                            step.Progress(currentStopwatch / (double) msecDuration, total);

                        } while (stopwatch.ElapsedMilliseconds <= msecDuration);
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

        
        private void CheckODirect()
        {
            _checkODirect.Start();

            _isODirectSupported = false;
            var firstFile = WorkingSet[0].FullName;
            try
            {
                _isODirectSupported = DiskBenchmarkChecks.IsO_DirectSupported_Readonly(firstFile, Parameters.RandomAccessBlockSize);
            }
            catch
            {
            }

            _checkODirect.Name = _isODirectSupported ? "Direct Access is detected" : "Direct Access is absent";
            _checkODirect.Complete();
        }

        
        private void SeqRead()
        {
            _seqRead.Start();
            CancelIfRequested();
            LinuxKernelCacheFlusher.Sync();
            
            byte[] buffer = new byte[1024 * 1024];
            long sumBytes = 0;
            _seqRead.Start();
            foreach (var fileInfo in WorkingSet)
            {
                CancelIfRequested();
                using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, buffer.Length))
                {
                    long len = 0;
                    long fileLen = fileInfo.Size;
                    while (len < fileLen)
                    {
                        CancelIfRequested();
                        var count = (int) Math.Min(Math.Min(fileLen - len, buffer.Length), Parameters.WorkingSetSize - sumBytes);
                        if (count <= 0) goto done;
                        
                        int n = fs.Read(buffer, 0, count);
                        if (n < 0) continue;
                        len += n;
                        sumBytes += n;
                        _seqRead.Progress(sumBytes / (double) Parameters.WorkingSetSize, sumBytes);
                    }
                }
                
                // Console.WriteLine($"SEQ READ: {sumBytes:n0} of {Parameters.WorkingSetSize:n0}");
            }
            
            done:
            _seqRead.Complete();
        }


        
        #region Analyze Metadata
        class FileInfo
        {
            public string FullName;
            public long Size;
            // 1 - random, 0.00001 - zero
            public double? ExpressCompression;

            public Stream Stream;
        }

        private void AnalyzeMetadata()
        {
            _analyze.Start();
            List<FileInfo> files = new List<FileInfo>();
            DirectoryInfo root = new DirectoryInfo(Parameters.WorkFolder);
            long totalSize = 0;
            
            // configure progress for single threaded analysis
            Stopwatch swProgress = Stopwatch.StartNew();
            long prevMsecs = -1000000000;
            Action<bool> progress = (isCompleted) =>
            {
                long msec = swProgress.ElapsedMilliseconds;
                long delta = msec - prevMsecs;
                if (delta > 222 || isCompleted)
                {
                    _analyze.Name = $"Analyze metadata {Formatter.FormatBytes(totalSize)}";
                    // Console.WriteLine($"Analyze metadata progress: {files.Count} files total size is {totalSize:n0} bytes");
                    prevMsecs = msec;
                }
            };

            bool abort = false;
            EnumDir(root, files, ref totalSize, () => progress(false), ref abort);
            progress(true);
            long bufferSize = 0;
            var query = files.OrderByDescending(x => x.Size).TakeWhile((file) =>
            {
                bufferSize += file.Size;
                return true || bufferSize < Parameters.WorkingSetSize;
            });

            WorkingSet = query.ToArray();
            var debugList = string.Join(
                Environment.NewLine,
                WorkingSet.Select((file,index) => $"  {Formatter.FormatBytes(file.Size):-11} {file.FullName} ({(index+1)})")
            );
            
            Console.WriteLine($"Working set for readonly benchmark of {Parameters.WorkFolder}{Environment.NewLine}{debugList}");
            
            if (totalSize < Parameters.WorkingSetSize)
                throw new Exception($"Insufficient content for readonly disk benchmark. Requested working set {Formatter.FormatBytes(Parameters.WorkingSetSize)}, but available is just {Formatter.FormatBytes(totalSize)}");

            _analyze.Complete();
        }

        void EnumDir(DirectoryInfo dir, List<FileInfo> result, ref long totalSize, Action progress, ref bool abort)
        {
            System.IO.FileInfo[] files = null;
            CancelIfRequested();
            try
            {
                files = dir.GetFiles();
            }
            catch 
            {
            }

            if (files != null)
            {
                foreach (var file in files)
                {
                    CancelIfRequested();
                    var fileFullName = file.FullName;
                    var len = file.Length;
                    if (len >= UnconditionalThreshold)
                    {
                        if (FileSystemHelper.IsSymLink(fileFullName)) continue;
                        if (!CanReadFile(fileFullName)) continue;

                        result.Add(new FileInfo() {Size = len, FullName = fileFullName});
                        totalSize += len;
                        progress();
                        if (IsJit && totalSize > Parameters.WorkingSetSize)
                        {
                            abort = true;
                            return;
                        }
                    }
                }
            }

            DirectoryInfo[] subDirs = null;
            try
            {
                subDirs = dir.GetDirectories();
            }
            catch
            {
            }

            if (subDirs != null)
            {
                foreach (var subDir in subDirs)
                {
                    CancelIfRequested();
                    if (FileSystemHelper.IsSymLink(subDir.FullName)) continue;
                    if (!IsBelongToVolume(subDir.FullName)) continue;
                    EnumDir(subDir, result, ref totalSize, progress, ref abort);
                    if (abort) return;
                }
            }

        }
        
        
        string[] _NormalizerMountPaths;
        string[] GetNormalizerMountPaths()
        {
            if (Mounts == null) return null; 
            if (_NormalizerMountPaths == null)
            {
                var pathSeparator = Path.DirectorySeparatorChar.ToString();
                Func<DriveDetails, string> getNormalizedMountPath = vol =>
                {
                    if (vol.MountEntry != null && vol.MountEntry.MountPath != null &&
                        !vol.MountEntry.MountPath.EndsWith(pathSeparator))
                        return vol.MountEntry.MountPath + pathSeparator;

                    return vol.MountEntry?.MountPath;
                };

                string normalizedWorkFolder = Parameters.WorkFolder;
                if (!normalizedWorkFolder.EndsWith(pathSeparator)) normalizedWorkFolder += pathSeparator;

                string[] normalizedPaths = Mounts
                    .Select(x => getNormalizedMountPath(x))
                    .Where(x => x != null)
                    .OrderBy(x => x)
                    .ToArray();

                normalizedPaths = normalizedPaths.Except(normalizedPaths.Where(normalizedWorkFolder.StartsWith)).ToArray();

                _NormalizerMountPaths = normalizedPaths;

#if DEBUG
                Console.WriteLine($@"Normalizer Mount Paths for readonly benchmark of [{Parameters.WorkFolder}]
{string.Join(Environment.NewLine, _NormalizerMountPaths.Select(x => $" {{{x}}}"))}");

#endif
            }

            return _NormalizerMountPaths;
        }
        

        private bool IsBelongToVolume(string subFolderFullName)
        {
                if (Mounts == null) return true;
                var pathSeparator = Path.DirectorySeparatorChar.ToString();

                var normalizedPaths = GetNormalizerMountPaths();
                string normalizedWorkFolder = Parameters.WorkFolder;
                if (!normalizedWorkFolder.EndsWith(pathSeparator)) normalizedWorkFolder += pathSeparator;
                var normalizedSubFolderFullName = subFolderFullName;
                if (!normalizedSubFolderFullName.EndsWith(pathSeparator)) normalizedSubFolderFullName += pathSeparator;
                
                var anotherVolumes = normalizedPaths
                    .Where(mountPath => mountPath != subFolderFullName)
                    .Where(mountPath => mountPath != normalizedWorkFolder)
                    .Where(normalizedSubFolderFullName.StartsWith);
                
                var ret = !anotherVolumes.Any();
#if DEBUG
                Console.WriteLine($" - [{subFolderFullName}] belongs to {Parameters.WorkFolder}: {ret} ({string.Join("; ", anotherVolumes)})");
#endif
                return ret;
        }

        static bool CanReadFile(string fullName)
        {
            try
            {
                byte[] buffer = new byte[1];
                using (FileStream fs = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.Read, 1))
                {
                    fs.Read(buffer, 0, 1);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
        #endregion
        
    }
}