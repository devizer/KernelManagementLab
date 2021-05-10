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

        public static Lazy<string> PosixMachine = new Lazy<string>(GetPosixMachine);
        public static Lazy<string> PosixSystem = new Lazy<string>(GetPosixSystem);
        public static Lazy<int> PosixLongBits = new Lazy<int>(GetPosixBits);

        public static List<Info> GetCandidates()
        {
            List<string> urls = new List<string>();
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
                
            }
            

            return urls.Select(Url2Info).ToList();
        }

        public static List<Info> AllMacOsCandidates()
        {
            List<string> urls = new List<string>();
            urls.Add("https://master.dl.sourceforge.net/project/fio/fio-3.16-amd64-darwin_10_10_5.gz?viasf=1");
            urls.Add("https://master.dl.sourceforge.net/project/fio/fio-2.21-amd64-darwin_10_10_5.gz?viasf=1");
            return urls.Select(Url2Info).ToList();
        }
        
        public static List<Info> AllWindowsCandidates()
        {
            List<string> urls = new List<string>();
            var url32 = "https://master.dl.sourceforge.net/project/fio/fio-3.25-x86-windows.exe.gz?viasf=1";
            var url64 = "https://master.dl.sourceforge.net/project/fio/fio-3.25-x64-windows.exe.gz?viasf=1";
            urls.Add(url64);
            urls.Add(url32);
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
            return LinuxSimpleLaunch("uname", "-m");
        }

        static string GetPosixSystem()
        {
            return LinuxSimpleLaunch("uname", "-s");
        }
        
        // 32|64
        static int GetPosixBits()
        {
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

            return pl.OutputText;
        }
    }
}