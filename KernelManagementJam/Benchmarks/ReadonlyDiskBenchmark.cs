using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using KernelManagementJam;
using KernelManagementJam.Benchmarks;
using Universe.DiskBench;

namespace Universe.Benchmark.DiskBench
{
    public class ReadonlyDiskBenchmark : IDiskBenchmark
    {
        
        private const int UnconditionalThreshold = 16384;
        public ReadonlyDiskBenchmarkOptions Parameters { get; set; }
        private FileInfo[] WorkingSet = null;
        
        public ProgressInfo Progress { get; private set; }
        
        private ProgressStep _analyze;
        private ProgressStep _checkODirect;
        private ProgressStep _seqRead;
        private ProgressStep _rndRead1T;
        private ProgressStep _rndReadN;
        private bool _isODirectSupported;

        public ReadonlyDiskBenchmark(ReadonlyDiskBenchmarkOptions parameters)
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
            LinuxKernelCacheFlusher.Sync();
            _seqRead.Start();
            
            byte[] buffer = new byte[1024 * 1024];
            long totalLen = 0;
            foreach (var fileInfo in WorkingSet)
            {
                
                using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, buffer.Length))
                {
                    _seqRead.Start();
                    long len = 0;
                    long fileLen = fileInfo.Size;
                    while (len < fileLen)
                    {
                        var count = (int) Math.Min(fileLen - len, buffer.Length);
                        int n = fs.Read(buffer, 0, count);
                        if (n < 0) continue;
                        len += n;
                        totalLen += n;
                        _seqRead.Progress(totalLen / (double) Parameters.WorkingSetSize, totalLen);
                    }
                }

            }
            
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
                if (delta > 100)
                {
                    _analyze.Name = $"Analyze metadata {Formatter.FormatBytes(totalSize)}";
                    prevMsecs = msec;
                }
            };
            
            EnumDir(root, files, ref totalSize, progress);
            long bufferSize = 0;
            var query = files.OrderByDescending(x => x.Size).TakeWhile((file) =>
            {
                bufferSize += file.Size;
                return bufferSize < Parameters.WorkingSetSize;
            });

            WorkingSet = query.ToArray();
            var debugList = string.Join(
                Environment.NewLine,
                WorkingSet.Select(x => $"  {Formatter.FormatBytes(x.Size):-11} {x.FullName}")
            );
            
            Console.WriteLine($"Working set for readonly benchmark of {Parameters.WorkFolder}{Environment.NewLine}{debugList}");
            _analyze.Complete();
            
            if (totalSize < Parameters.WorkingSetSize)
                throw new Exception($"Insufficient content for readonly disk benchmark. Requested working set {Formatter.FormatBytes(Parameters.WorkingSetSize)}, but available is just {Formatter.FormatBytes(totalSize)}");
        }

        static void EnumDir(DirectoryInfo dir, List<FileInfo> result, ref long totalSize, Action progress)
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
                    var len = file.Length;
                    if (len >= UnconditionalThreshold)
                    {
                        result.Add(new FileInfo() {Size = len, FullName = file.FullName});
                        totalSize += len;
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
                    EnumDir(subDir, result, ref totalSize, progress);
                }
            }

        }
        #endregion
        
    }
}