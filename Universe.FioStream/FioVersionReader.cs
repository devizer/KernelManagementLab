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
                Action<StreamReader> handler = streamReader =>
                {
                    rawVersion = streamReader.ReadToEnd();
                    rawVersion = rawVersion.Trim('\r', '\n');
                };

                FioLauncher launcher = new FioLauncher(this.Executable, new[] {"--version"}, handler);
                launcher.Start();

                if (launcher.ExitCode != 0 || !string.IsNullOrEmpty(launcher.ErrorText))
                    throw new Exception($"Version failed: Exit Code [{launcher.ExitCode}]. ERROR TEXT: [{launcher.ErrorText}]");
                
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

            return new Version(ret.ToString());
        }

    }
}