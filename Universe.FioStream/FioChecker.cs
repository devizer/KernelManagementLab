using System;
using System.Diagnostics;
using System.IO;

namespace Universe.FioStream
{
    public class FioChecker
    {
        public string Executable { get; }
        public IPicoLogger Logger { get; set; }

        public FioChecker(string executable)
        {
            Executable = executable;
        }

        public JobSummaryResult CheckBenchmark(params string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            JobSummaryResult ret = null;
            try
            {
                void Handler(StreamReader streamReader)
                {
                    FioStreamReader rdr = new FioStreamReader();
                    rdr.NotifyJobSummary += summary => { ret = summary; };
                    rdr.ReadStreamToEnd(streamReader);
                }

                FioLauncher launcher = new FioLauncher(Executable, args, Handler);
                launcher.Start();
                if (!string.IsNullOrEmpty(launcher.ErrorText) || launcher.ExitCode != 0)
                {
                    var err = launcher.ErrorText.TrimEnd('\r', '\n');
                    Logger?.LogWarning($"Fio benchmark test failed for [{Executable}]. Exit Code [{launcher.ExitCode}]. Error: [{err}]");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogWarning($"Fio benchmark test failed for [{Executable}]. {ex.GetExceptionDigest()}");
                return null;
            }

            Logger?.LogInfo($"Fio benchmark test results looks good for [{Executable}]");
            return ret;
        }

        public Version CheckVersion()
        {
            FioVersionReader vr = new FioVersionReader(Executable);
            try
            {
                var verText = vr.GetTextVersion();
                var ret = vr.GetVersion();
                Logger?.LogInfo($"Fio version test passed for [{Executable}] version [{ret}]");
                return ret;
            }
            catch (Exception ex)
            {
                var msg = ex.GetExceptionDigest();
                Logger?.LogInfo($"Warning. Fio version test failed for [{Executable}]: '{msg}'");
                return null;
            }
        }
    }
}