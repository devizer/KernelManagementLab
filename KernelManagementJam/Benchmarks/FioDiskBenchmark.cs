using System;
using System.Diagnostics;
using System.IO;
using Universe.Benchmark.DiskBench;
using Universe.DiskBench;
using Universe.FioStream;
using Universe.FioStream.Binaries;

namespace KernelManagementJam.Benchmarks
{
    public class FioDiskBenchmark : IDiskBenchmark
    {
        public DiskBenchmarkOptions Parameters { get; }
        public FioEnginesProvider.Engine Engine { get; set; }
        public ProgressInfo Progress { get; private set; }
        public static readonly string BenchmarkTempFile = DiskBenchmark.BenchmarkTempFile;
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

        public FioDiskBenchmark(DiskBenchmarkOptions parameters)
        {
            Parameters = parameters;

            // copy/pasta from another ctor
            var workFolderFullName = new DirectoryInfo(Parameters.WorkFolder).FullName;
            TempFile = Path.Combine(workFolderFullName, BenchmarkTempFile);
            BuildProgress();
        }
        
        void BuildProgress()
        {
            if (Parameters.DisableODirect)
            {
                _checkODirect = new ProgressStep($"Direct Access is disabled, {Engine.IdEngine} v{Engine.Version}") { Value = null};
                _isODirectSupported = false;
                _checkODirect.Start();
                _checkODirect.Complete();
            }
            else
            {
                _checkODirect = new ProgressStep("Check capabilities");
            }

            _checkODirect.Column = ProgressStepHistoryColumn.CheckODirect;
                
            _allocate = new ProgressStep($"Allocate {Formatter.FormatBytes(Parameters.WorkingSetSize)}") {Column = ProgressStepHistoryColumn.Allocate};
            _seqRead = new ProgressStep("Sequential read"){ CanHaveMetrics = true, Column = ProgressStepHistoryColumn.SeqRead};
            _seqWrite = new ProgressStep("Sequential write"){CanHaveMetrics = true, Column = ProgressStepHistoryColumn.SeqWrite};
            _rndRead1T = new ProgressStep("Random Read, 1 thread"){CanHaveMetrics = true, Column = ProgressStepHistoryColumn.RandRead1T};
            _rndWrite1T = new ProgressStep("Random Write, 1 thread"){CanHaveMetrics = true, Column = ProgressStepHistoryColumn.RandWrite1T};
            _rndReadN = new ProgressStep($"Random Read, {Parameters.ThreadsNumber} threads"){CanHaveMetrics = true, Column = ProgressStepHistoryColumn.RandReadNT};
            _rndWriteN = new ProgressStep($"Random Write, {Parameters.ThreadsNumber} threads"){CanHaveMetrics = true, Column = ProgressStepHistoryColumn.RandWriteNT};
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

        private FileOptions GenericWritingFileStreamOptions =>
            FileOptions.WriteThrough;

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
            
            Action<Exception> doCleanUp = (ex) =>
            {
                _cleanUp.Start();
                if (File.Exists(TempFile))
                    File.Delete(TempFile);

                if (ex != null) _cleanUp.Name = $"Benchmark failed. {ex.GetExceptionDigest()}";
                _cleanUp.Complete();
            };

            try
            {
                if (!Parameters.DisableODirect) CheckODirect();
                Allocate();
                
                DoFioBenchmark(_seqRead, Parameters.Engine, "read", _isODirectSupported, "1024k", 0);
                DoFioBenchmark(_seqWrite, Parameters.Engine, "write", _isODirectSupported, "1024k", 0);
                DoFioBenchmark(_rndRead1T, Parameters.Engine,"randread", _isODirectSupported, Parameters.RandomAccessBlockSize.ToString("0"), 1);
                DoFioBenchmark(_rndWrite1T, Parameters.Engine,"randwrite", _isODirectSupported, Parameters.RandomAccessBlockSize.ToString("0"), 1);
                DoFioBenchmark(_rndReadN, Parameters.Engine,"randread", _isODirectSupported, Parameters.RandomAccessBlockSize.ToString("0"), 64);
                DoFioBenchmark(_rndWriteN, Parameters.Engine,"randwrite", _isODirectSupported, Parameters.RandomAccessBlockSize.ToString("0"), 64);
                
                doCleanUp(null);
            }
            catch(Exception ex)
            {
                doCleanUp(ex);
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

                _cleanUp.State = ProgressStepState.Error;
                Console.WriteLine($"Benchmark for [{Parameters.WorkFolder}] failed. {ex.GetExceptionDigest()}{Environment.NewLine}{ex}");
                throw;
            }
        }

        private void DoFioBenchmark(ProgressStep step, string engine, string command, bool needDirectIo, string blockSize, int ioDepth, string options = "--eta=always --time_based ")
        {
            CancelIfRequested();
            string workingDirectory = Path.GetDirectoryName(this.TempFile);
            string fileName = Path.GetFileName(this.TempFile);

            bool hasBlockSize = !string.IsNullOrEmpty(blockSize);
            bool hasIoDepth = ioDepth > 0;

            string args = options + 
                          $" --name=RUN_{command}" +
                          $" --ioengine={engine}" +
                          $" --direct={(needDirectIo ? "1" : "0")}" +
                          $" --gtod_reduce=1" +
                          $" --filename={fileName}" +
                          (hasBlockSize ? $" --bs={blockSize}" : "")  +
                          (hasIoDepth ? $" --iodepth={ioDepth}" : "") +
                          $" --size={Parameters.WorkingSetSize:0}" +
                          $" --runtime={(Parameters.StepDuration / 1000)}" +
                          $" --ramp_time=0" +
                          $" --readwrite={command}";
            
            Stopwatch startAt = null;
            void Handler(StreamReader streamReader)
            {
                FioStreamReader rdr = new FioStreamReader();
                rdr.NotifyEta += eta =>
                {
                    CancelIfRequested();
                    Console.WriteLine($"---=== ETA {eta} ===---");
                };
                rdr.NotifyJobProgress += progress =>
                {
                    CancelIfRequested();
                    startAt = startAt ?? Stopwatch.StartNew();
                    var bandwidth = progress.ReadBandwidth.GetValueOrDefault() + progress.WriteBandwidth.GetValueOrDefault();
                    double elapsedSeconds = startAt.Elapsed.TotalSeconds;
                    double percents = 1000 * elapsedSeconds / Parameters.StepDuration;
                    var totalBytes = bandwidth * elapsedSeconds; 
                    var seconds = step.Seconds;
                    
                    Console.WriteLine($"---=== PROGRESS [{progress}] ===---");

                    var @break = @"here";

                    step.Progress(percents, (long) totalBytes);
                };
                rdr.NotifyJobSummary += summary =>
                {
                    var bandwidth = summary.Bandwidth;
                    double elapsedSeconds = startAt.Elapsed.TotalSeconds;
                    double percents = 1000 * elapsedSeconds / Parameters.StepDuration;
                    var totalBytes = bandwidth * elapsedSeconds; 
                    step.Progress(percents, (long) totalBytes);
                    Console.WriteLine($"---=== SUMMARY [{summary}] ===---");
                };
                rdr.ReadStreamToEnd(streamReader);
            }

            FioLauncher launcher = new FioLauncher(Engine.Executable, args, Handler)
            {
                WorkingDirectory = workingDirectory
            };
            
            startAt = Stopwatch.StartNew();
            step.Start();
            CancelIfRequested();
            launcher.Start();
            if (!string.IsNullOrEmpty(launcher.ErrorText) || launcher.ExitCode != 0)
            {
                var err = launcher.ErrorText?.TrimEnd('\r', '\n');
                var msg = $"Fio benchmark test failed for [{Engine}]. Exit Code [{launcher.ExitCode}]. Error: [{err}]. Args: [{args}]. Working Directory [{workingDirectory ?? "<current>"}]";
                throw new Exception(msg);
            }
            
            step.Complete();

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
        
        private void CheckODirect()
        {
            _checkODirect.Start();

            _isODirectSupported = false;
            try
            {
                _isODirectSupported = DiskBenchmarkChecks.IsO_DirectSupported(Parameters.WorkFolder, 128 * 1024);
            }
            catch
            {
            }

            _checkODirect.Value = _isODirectSupported; 

            _checkODirect.Name = (_isODirectSupported ? "Direct Access is detected" : "Direct Access is absent") + $", {Engine.IdEngine} v{Engine.Version}";
            _checkODirect.Complete();
        }
        
        private void Allocate()
        {
            _allocate.Start();
            LinuxKernelCacheFlusher.Sync();
            byte[] buffer = new byte[Math.Min(10 * 1024 * 1024, this.Parameters.WorkingSetSize)];
            new DataGenerator(Parameters.Flavour).NextBytes(buffer);
            CpuUsageInProgress cpuUsage = CpuUsageInProgress.StartNew();
            using (FileStream fs = new FileStream(TempFile, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, GenericWritingFileStreamOptions))
            {
                if (false) 
                {
                    // For ext/btrfs has no essect
                    // For fat/fat32 - too slow , inappropriate 
                    fs.Position = Parameters.WorkingSetSize - 1;
                    fs.WriteByte(0);
                    fs.Position = 0;
                }
                _allocate.Start();
                
                long len = 0;
                while (len < this.Parameters.WorkingSetSize)
                {
                    CancelIfRequested();
                    var count = (int) Math.Min(this.Parameters.WorkingSetSize - len, buffer.Length);
                    fs.Write(buffer, 0, count);
                    len += count;
                    _allocate.Progress(len / (double) Parameters.WorkingSetSize, len);
                    if (cpuUsage.AggregateCpuUsage(force: false))
                        _allocate.CpuUsage = cpuUsage.Result;
                }
                _allocate.Complete();
                cpuUsage.AggregateCpuUsage(force: true);
                _allocate.CpuUsage = cpuUsage.Result;
            }
            LinuxKernelCacheFlusher.Sync();
        }





    }
}