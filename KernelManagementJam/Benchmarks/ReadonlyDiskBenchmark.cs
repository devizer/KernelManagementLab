using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KernelManagementJam;
using KernelManagementJam.Benchmarks;
using Universe.DiskBench;

namespace Universe.Benchmark.DiskBench
{
    public class ReadonlyDiskBenchmark : IDiskBenchmark
    {
        
        public DiskBenchmarkOptions Parameters { get; set; }
        public bool IsJit = false;
        
        private const int UnconditionalThreshold = 16384;
        private FileInfo[] WorkingSet = null;
        
        public ProgressInfo Progress { get; private set; }
        
        private ProgressStep _analyze;
        private ProgressStep _checkODirect;
        private ProgressStep _seqRead;
        private ProgressStep _rndRead1T;
        private ProgressStep _rndReadN;
        private bool _isODirectSupported;

        public ReadonlyDiskBenchmark(DiskBenchmarkOptions parameters)
        {
            Parameters = parameters;
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
            finally
            {
                Progress.IsCompleted = true;
            }
        }

        private void Perform_Impl()
        {
            Random random = new Random();
            AnalyzeMetadata();
            SeqRead();
        }
        
        private void SeqRead()
        {
            _seqRead.Start();
            LinuxKernelCacheFlusher.Sync();
            
            byte[] buffer = new byte[1024 * 1024];
            long sumBytes = 0;
            _seqRead.Start();
            foreach (var fileInfo in WorkingSet)
            {
                
                using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, buffer.Length))
                {
                    long len = 0;
                    long fileLen = fileInfo.Size;
                    while (len < fileLen)
                    {
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
            Action progress = () =>
            {
                long msec = swProgress.ElapsedMilliseconds;
                long delta = msec - prevMsecs;
                if (delta > 222)
                {
                    _analyze.Name = $"Analyze metadata {Formatter.FormatBytes(totalSize)}";
                    // Console.WriteLine($"Analyze metadata progress: {files.Count} files total size is {totalSize:n0} bytes");
                    prevMsecs = msec;
                }
            };

            bool abort = false;
            EnumDir(root, files, ref totalSize, progress, ref abort);
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
            _analyze.Complete();
            
            if (totalSize < Parameters.WorkingSetSize)
                throw new Exception($"Insufficient content for readonly disk benchmark. Requested working set {Formatter.FormatBytes(Parameters.WorkingSetSize)}, but available is just {Formatter.FormatBytes(totalSize)}");
        }

        void EnumDir(DirectoryInfo dir, List<FileInfo> result, ref long totalSize, Action progress, ref bool abort)
        {
            System.IO.FileInfo[] files = null;
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
                    if (FileSystemHelper.IsSymLink(subDir.FullName)) continue;
                    EnumDir(subDir, result, ref totalSize, progress, ref abort);
                    if (abort) return;
                }
            }

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