using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Universe.FioStream
{
    public class FioEngineListReader
    {
        public string Executable { get; }

        public FioEngineListReader(string executable)
        {
            Executable = executable;
        }

        public string[] GetEngileList()
        {
            List<string> ret = new List<string>();
            Action<StreamReader> handler = streamReader =>
            {
                var allRaw = streamReader.ReadToEnd();
                allRaw = allRaw.Trim('\r', '\n');
                var lines = allRaw.Split('\r', '\n', '\t')
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToArray();

                foreach (var line in lines)
                {
                    if (line.IndexOf(' ') < 0)
                        ret.Add(line);
                }
            };
            
            FioLauncher launcher = new FioLauncher(this.Executable, new[] {"--enghelp"}, handler);
            launcher.Start();

            if (launcher.ExitCode != 0 || !string.IsNullOrEmpty(launcher.ErrorText))
            {
                var err = launcher.ErrorText.TrimEnd('\r', '\n');
                throw new Exception($"Engine List failed: Exit Code [{launcher.ExitCode}]. ERROR TEXT: [{err}]");
            }

            if (ret.Count == 0)
                throw new Exception($"Empty engine list received for '{Executable}'");

            return ret.ToArray();
        }
    }
}