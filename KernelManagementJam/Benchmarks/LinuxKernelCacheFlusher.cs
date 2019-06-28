using System;
using System.Diagnostics;
using System.IO;

namespace KernelManagementJam.Benchmarks
{
    public class LinuxKernelCacheFlusher
    {
        private static bool SkipFlushing => Environment.GetEnvironmentVariable("SKIP_FLUSHING") == "true";
        public static void Sync()
        {
            if (SkipFlushing) return;
            SyncWriteBuffer();
            FlushReadBuffers();
        }

        public static void SyncWriteBuffer()
        {
            StartAndIgnore("sync", "");
        }

        public static void FlushReadBuffers()
        {
            bool isDropOk = false;
            try
            {
                File.WriteAllText("/proc/sys/vm/drop_caches", "1");
                isDropOk = true;
            }
            catch
            {
            }

            if (!isDropOk)
            {
                StartAndIgnore("sudo", "sh -c \"echo 1 > /proc/sys/vm/drop_caches\"");
            }
        }
        
        static int StartAndIgnore(string fileName, string args)
        {
            Stopwatch sw = Stopwatch.StartNew(); 
            string info = fileName == "sudo" ? args : fileName;
                
            try
            {
                ProcessStartInfo si = new ProcessStartInfo(fileName, args);
                si.RedirectStandardError = true;
                si.RedirectStandardOutput = true;
                using (Process p = Process.Start(si))
                {
                    p.Start();
                    p.WaitForExit();
#if DEBUG
                    Console.WriteLine($"Process [{info}] successfully finished in {sw.ElapsedMilliseconds:n0} milliseconds");
#endif
                    return p.ExitCode;
                }
            }
            catch
            {
#if DEBUG
                Console.WriteLine($"Process [{info}] failed in {sw.ElapsedMilliseconds:n0} milliseconds");
#endif
                return -1;
            }
        }

    }
}