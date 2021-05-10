using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Universe.FioStream.Binaries
{
    public class ProcessLauncher
    {
        public string Executable { get; } 
        public string[] Args { get; }

        public string ErrorText { get; private set; }
        public string OutputText { get; private set; }
        public Exception OutputReaderException { get; private set; }
        public Exception ErrorReaderException { get; private set; }
        public int ExitCode { get; private set; }

        public ProcessLauncher(string executable, params string[] args)
        {
            Executable = executable;
            Args = args;
        }

        public void Start()
        {
            ProcessStartInfo si = new ProcessStartInfo(Executable, string.Join(" ", Args))
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                // WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
            };
            
            Process p = new Process() { StartInfo = si };
            
            // error is written to output xml
            ManualResetEventSlim outputDone = new ManualResetEventSlim(false);
            ManualResetEventSlim errorDone = new ManualResetEventSlim(false);

            string errorText = null, outputText = null;
            Exception my_outputException = null;
            Exception my_errorException = null;

            Thread threadErrorOutput = new Thread(() =>
                {
                    try
                    {
                        errorText = p.StandardError.ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                        my_errorException = ex;
                    }
                    finally
                    {
                        errorDone.Set();
                    }
                }
#if !NETSTANDARD1_3 && !NETCOREAPP1_0 && !NETCOREAPP1_1
                , 64 * 1024
#endif
            ) { IsBackground = true };

            Thread threadStandardOutput = new Thread(() =>
                    {
                        try
                        {
                            outputText = p.StandardOutput.ReadToEnd();
                        }
                        catch (Exception ex)
                        {
                            my_outputException = ex;
                        }
                        finally
                        {
                            outputDone.Set();
                        }
                    }
#if !NETSTANDARD1_3 && !NETCOREAPP1_0 && !NETCOREAPP1_1
                , 64 * 1024
#endif
                )
                {IsBackground = true};
            
            using (p)
            {
                p.Start();
                threadErrorOutput.Start();
                threadStandardOutput.Start();
                errorDone.Wait();
                outputDone.Wait();
                p.WaitForExit();
                ExitCode = p.ExitCode;
            }

            ErrorText = errorText;
            OutputText = outputText;
            OutputReaderException = my_outputException;
            ErrorReaderException = my_errorException;
        } 
        
    }
}