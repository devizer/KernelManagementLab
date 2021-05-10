using System;
using System.IO;

namespace Universe.FioStream
{
    public class FioChecker
    {
        public string Executable { get; } 

        public FioChecker(string executable)
        {
            Executable = executable;
        }

        public JobSummaryResult CheckBenchmark(params string[] args)
        {
            JobSummaryResult ret = null;
            try
            {
                Action<StreamReader> handler = streamReader =>
                {
                    FioStreamReader rdr = new FioStreamReader();
                    rdr.NotifyJobSummary += summary => { ret = summary; };
                    rdr.ReadStreamToEnd(streamReader);
                };

                FioLauncher launcher = new FioLauncher(Executable, args, handler);
                launcher.Start();
                if (!string.IsNullOrEmpty(launcher.ErrorText) || launcher.ExitCode != 0)
                {
                    var err = launcher.ErrorText.TrimEnd('\r', '\n');
                    throw new Exception(
                        $"Benchmark check failed for [{Executable}]. Exit Code [{launcher.ExitCode}]. Error: [{err}]");
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return ret;
        }

        public Version CheckVersion()
        {
            FioVersionReader vr = new FioVersionReader(Executable);
            try
            {
                var verText = vr.GetTextVersion();
                return vr.GetVersion();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}