using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Universe.FioStream.Binaries
{
    public class GZipCachedDownloader
    {

        public static bool IgnoreCacheForDebug = false;
        
        public IPicoLogger Logger { get; set; }

        public string CacheGZip(string name, string url)
        {
            try
            {
                var cached = CacheGZip_Impl(name, url);
                Logger?.LogInfo($"The '{url}' cached as [{cached}]");
                return cached;

            }
            catch (Exception ex)
            {
                Logger?.LogWarning($"The '{url}' can not be downloaded and extracted. {ex.GetType()}: {ex.Message}");
                throw;
            }
        }

        private string CacheGZip_Impl(string name, string url)
        {
            var cacheStamp = Path.Combine(PersistentState.StateFolder, $"{name}.state");
            if (IgnoreCacheForDebug && File.Exists(cacheStamp)) File.Delete(cacheStamp);
            var ret = Path.Combine(PersistentState.BinFolder, $"{name}");
            if (File.Exists(cacheStamp) && File.Exists(ret))
                return ret;

            var guid = Guid.NewGuid().ToString("N");
            var tempGZip = Path.Combine(PersistentState.TempFolder, $"{name}.{guid}.gzipped");
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
                ProcessLauncher launcher = new ProcessLauncher("chmod", "+x", $"\"{ret}\"");
                launcher.Start();
                if (launcher.ExitCode != 0)
                    throw new Exception($"Unable to set execution bit for '{ret}'. Exit Code {launcher.ExitCode}. Output: [{launcher.OutputText}]. Error: [{launcher.ErrorText}]");
            }

            try
            {
                File.Delete(tempGZip);
            }
            catch{}

            using(FileStream cs = new FileStream(cacheStamp, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using(StreamWriter wr = new StreamWriter(cs, new UTF8Encoding(false)))
            {
                wr.Write("{\"ok\":true}");
            }

            return ret;
        }

    }
}