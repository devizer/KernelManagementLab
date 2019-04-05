using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Universe.Dashboard.Agent;

namespace ReactGraphLab
{
    static class PidFileSupport
    {
        private static readonly string ServiceKey = "w3top";
        public static void CreatePidFile()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var title = $"{Path.GetDirectoryName(entryAssembly.Location)} ver {entryAssembly.GetName().Version}";
            var pidFile = Environment.GetEnvironmentVariable("PID_FILE_FULL_PATH");
            if (string.IsNullOrEmpty(pidFile))
            {
                var dir = "/var/run";
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    dir = Environment.GetEnvironmentVariable("APPDATA");

                pidFile = Path.Combine(dir, $"{ServiceKey}.pid");
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(pidFile));
            }
            catch
            {
            }

            try
            {
                using (FileStream fs = new FileStream(pidFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter wr = new StreamWriter(fs, Encoding.ASCII))
                {
                    wr.WriteLine(Process.GetCurrentProcess().Id);
                }
                Console.WriteLine($"PID file for {title} created: {pidFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to created PID file [{pidFile}]. SystemD monitoring of a {ServiceKey}.service may not work properly. {ex.GetExceptionDigest()}");
            }

            
        }
    }
}