using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Universe.FioStream.Binaries
{
    public class GZipCachedDownloader
    {

        public string CacheGZip(string name, string url)
        {
            var cacheStamp = Path.Combine(CacheFolder.Value, $"{name}.state");
            var ret = Path.Combine(BinFolder.Value, $"{name}");
            if (File.Exists(cacheStamp) && File.Exists(ret))
                return ret;

            var tempGZip = Path.Combine(CacheFolder.Value, $"{name}.gzipped");
            var wd = new WebDownloader();
            wd.Download(url, tempGZip);
            
            using(FileStream from = new FileStream(tempGZip, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using(GZipStream unpack = new GZipStream(from, CompressionMode.Decompress))
            using(FileStream to = new FileStream(ret, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                unpack.CopyTo(to);
            }

            if (CrossInfo.ThePlatform != CrossInfo.Platform.Windows)
            {
                ProcessLauncher launcher = new ProcessLauncher("chmod", "+x", $"'{ret}'");
                launcher.Start();
                if (launcher.ExitCode != 0)
                    throw new Exception($"Unable to set execution bit for '{ret}'");
            }

            try
            {
                File.Delete(tempGZip);
            }
            catch{}

            using(FileStream cs = new FileStream(cacheStamp, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using(StreamWriter wr = new StreamWriter(cs, new UTF8Encoding(false)))
            {
                wr.Write("{ok:true}");
            }

            return ret;
        }

        private static Lazy<string> CacheFolder = new Lazy<string>(() =>
        {
            return GetDir($".local{Path.DirectorySeparatorChar}tmp");
        });

        private static Lazy<string> BinFolder = new Lazy<string>(() =>
        {
            return GetDir($".local{Path.DirectorySeparatorChar}bin");
        });

        static string GetDir(string subFolder)
        {
            List<string> candidates = new List<string>();
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
            {
#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_3
                candidates.Add(Environment.GetEnvironmentVariable("LOCALAPPDATA"));
                candidates.Add(Environment.GetEnvironmentVariable("APPDATA"));
#else
                candidates.Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                candidates.Add(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
#endif
                candidates.Add(Environment.GetEnvironmentVariable("HOMEPATH"));
                candidates.Add(Environment.GetEnvironmentVariable("USERPROFILE"));
                candidates.Add(Environment.GetEnvironmentVariable("TEMP"));
            }
            else
            {
                candidates.Add(Environment.GetEnvironmentVariable("HOME"));
                candidates.Add(Environment.GetEnvironmentVariable("TMPDIR"));
                candidates.Add(Environment.GetEnvironmentVariable("/tmp"));
            }

            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate))
                {
                    var fullPath = Path.Combine(candidate, subFolder);
                    try
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    catch
                    {
                    }

                    if (Directory.Exists(fullPath)) return fullPath;
                }
            }

            throw new Exception($"Unable to create local directory {subFolder}");
        }
    }
}