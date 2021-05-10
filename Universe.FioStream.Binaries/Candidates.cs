using System;

namespace Universe.FioStream.Binaries
{
    public class Candidates
    {
        

        string GetLinuxMachine()
        {
            return LinuxSimpleLaunch("uname", "-m");
        }
        
        // 32|64
        int GetLinuxBits()
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
                return IntPtr.Size;
            }

            return IntPtr.Size;
        }
        


        string LinuxSimpleLaunch(string proc, params string[] args)
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