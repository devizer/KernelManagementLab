using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Universe.FioStream.Binaries
{
    public class Candidates
    {
        
        public class Info
        {
            public string Url { get; set; }
            public string Name { get; set; }
        }

        static Lazy<string> _PosixMachine = new Lazy<string>(GetPosixMachine);
        static Lazy<string> _PosixSystem = new Lazy<string>(GetPosixSystem);
        static Lazy<int> _PosixLongBits = new Lazy<int>(GetPosixBits);
        static Lazy<Version> _LibCVersion = new Lazy<Version>(GetLibCVersion);

        public static string PosixMachine = _PosixMachine.Value;
        public static string PosixSystem = _PosixSystem.Value;
        public static int PosixLongBits = _PosixLongBits.Value;
        public static Version LibCVersion = _LibCVersion.Value;

        public static List<Info> GetCandidates()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
            {
                return AllWindowsCandidates();
            }
            else if (CrossInfo.ThePlatform == CrossInfo.Platform.MacOSX)
            {
                return AllMacOsCandidates();
            }
            else
            {
                var ret = OrderedLinuxCandidates.FindCandidateByLinuxMachine(PosixMachine);

                return ret.Select(x => new Info()
                {
                    Name = x.Name,
                    Url = x.Url
                }).ToList();
            }
        }

        public static List<Info> AllMacOsCandidates()
        {
            List<string> urls = new List<string>()
            {
                "https://master.dl.sourceforge.net/project/fio/fio-3.16-amd64-darwin_10_10_5.gz?viasf=1",
                "https://master.dl.sourceforge.net/project/fio/fio-2.21-amd64-darwin_10_10_5.gz?viasf=1"
            };
            return urls.Select(Url2Info).ToList();
        }
        
        public static List<Info> AllWindowsCandidates()
        {
            List<string> urls = new List<string>()
            {
                "https://master.dl.sourceforge.net/project/fio/fio-3.25-x64-windows.exe.gz?viasf=1",
                "https://master.dl.sourceforge.net/project/fio/fio-3.25-x86-windows.exe.gz?viasf=1"
            };
            if (IntPtr.Size == 32) urls.Reverse();
            return urls.Select(Url2Info).ToList();
        }

        static Info Url2Info(string url)
        {
            return new Info()
            {
                Name = Path.GetFileNameWithoutExtension(new Uri(url).AbsolutePath),
                Url = url
            };
        }

        static string GetPosixMachine()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows) return IntPtr.Size == 8 ? "64-bit" : "32-bit";
            return LinuxSimpleLaunch("uname", "-m");
        }

        static string GetPosixSystem()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows) return "Windows";
            return LinuxSimpleLaunch("uname", "-s");
        }
        
        // 32|64
        static int GetPosixBits()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows) return IntPtr.Size * 8;
            try
            {
                var raw = LinuxSimpleLaunch("getconf", "LONG_BIT");
                if (!string.IsNullOrEmpty(raw))
                {
                    if (int.TryParse(raw, out var ret))
                    {
                        return ret;
                    }
                }
            }
            catch (Exception ex)
            {
                return IntPtr.Size * 8;
            }

            return IntPtr.Size * 8;
        }

        static Version GetLibCVersion()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows) return null;
            try
            {
                var raw = LinuxSimpleLaunch("ldd", "--version");
                string firstLine = raw.Split('\r', '\n').FirstOrDefault();
                string lastWord = firstLine?.Split(' ').LastOrDefault(); 
                if (!string.IsNullOrEmpty(lastWord))
                {
                    return new Version(lastWord);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        static string LinuxSimpleLaunch(string proc, params string[] args)
        {
            ProcessLauncher pl = new ProcessLauncher(proc, args);
            try
            {
                pl.Start();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fail: {proc}.", ex);
            }

            if (pl.ExitCode != 0)
                throw new Exception($"Fail: {proc}. Exit code {pl.ExitCode}");

            return pl.OutputText.TrimEnd('\r', '\n');
        }
    }
}