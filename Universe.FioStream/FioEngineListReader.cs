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

        public string[] GetEngineList()
        {
            // cpuio-,mmap+,sync+,psync+,vsync+,pvsync+,pvsync2+,null+,net-,netsplice-,ftruncate-,posixaio+,falloc+,e4defrag-,splice+,mtd-,sg-,binject-
            // ,mmap+,sync+,psync+,vsync+,pvsync+,pvsync2+,null+,net-,netsplice-,ftruncate-,filecreate+,filestat+,posixaio+,falloc+,e4defrag-,splice+,mtd-,sg-,io_uring+
            // io_uring, libaio, posixaio, pvsync2, pvsync, vsync, psync, sync, mmap
            List<string> ret = new List<string>();

            void Handler(StreamReader streamReader)
            {
                var allRaw = streamReader.ReadToEnd();
                allRaw = allRaw.Trim('\r', '\n');
                var lines = allRaw.Split('\r', '\n', '\t')
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToArray();

                foreach (var line in lines)
                {
                    if (line.IndexOf(' ') < 0) ret.Add(line);
                }
            }

            FioLauncher launcher = new FioLauncher(this.Executable, "--enghelp", Handler);
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