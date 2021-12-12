using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Universe.FioStream
{
    public class FioVersionReader
    {
        public string Executable { get; }

        private Lazy<string> TextVersion;

        private FioVersionReader()
        {
            TextVersion = new Lazy<string>(() =>
            {
                string rawVersion = null;

                void Handler(StreamReader streamReader)
                {
                    rawVersion = streamReader.ReadToEnd();
                    rawVersion = rawVersion.Trim('\r', '\n');
                }

                FioLauncher launcher = new FioLauncher(this.Executable, "--version", Handler);
                launcher.Start();

                if (launcher.ExitCode != 0 || !string.IsNullOrEmpty(launcher.ErrorText))
                {
                    var err = launcher.ErrorText.TrimEnd('\r', '\n');
                    throw new Exception($"Version failed: Exit Code [{launcher.ExitCode}]. ERROR TEXT: [{err}]");
                }

                return rawVersion;

            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public FioVersionReader(string executable) : this()
        {
            Executable = executable;
        }

        public string GetTextVersion() => TextVersion.Value;

        public Version GetVersion()
        {
            var textVersion = GetTextVersion();
            var ret = StripVersionText(textVersion);
            return new Version(ret);
        }
        
        static string StripVersionText(string textVersion)
        {
            StringBuilder ret = new StringBuilder();
            foreach (var c in textVersion)
            {
                bool isIt = c == '.' || (c >= '0' && c <= '9');
                if (isIt)
                {
                    ret.Append(c);
                }
                else
                {
                    if (ret.Length > 0) break;
                }
            }

            return ret.ToString();
        }


    }
}