using System;
using System.IO;

namespace Universe.FioStream.Binaries
{
    public class FioFeatures
    {
        public string Executable { get; }
        public IPicoLogger Logger { get; set; }

        public FioFeatures(string executable)
        {
            Executable = executable;
        }

        public Version Version
        {
            get
            {
                var textVersion = PersistentState.GetOrStore($"{Executable}-Ver", () => 
                {
                    FioChecker checker = new FioChecker(Executable) {Logger = Logger};
                    Version ver = checker.CheckVersion();
                    return ver == null ? null : ver.ToString();
                });

                return textVersion == null ? null : new Version(textVersion);
            }
        }

        public string[] EngineList
        {
            get
            {
                return PersistentState.GetOrStore($"{Executable}-Engines", () =>
                {
                    string[] engines;
                    try
                    {
                        FioEngineListReader rdr = new FioEngineListReader(Executable);
                        engines = rdr.GetEngineList();
                        return engines;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Can't obtain engine list for [{Executable}]. {ex.Message}");
                    }

                    return null;
                });
            }
        }

        public bool IsEngineSupported(string engine)
        {
            Random rand = new Random();
            var dir = PersistentState.TempFolder;
            string file = null, fileFull;
            do
            {
                file = $"fiobench.{rand.Next(999):000}";
                fileFull = Path.Combine(dir, file);
                if (!File.Exists(fileFull)) break;
            } while (true);

            string isSupported;
            try
            {
                isSupported = PersistentState.GetOrStore($"{Executable}-Engine-{engine}", () =>
                {
                    FioChecker checker = new FioChecker(Executable) {Logger = Logger};
                    JobSummaryResult summary = checker.CheckBenchmark("--name=my", "--bs=8k", "--size=8k", $"--ioengine={engine}",
                        $"\"--directory={dir}\"", $"--filename={file}");
                    
                    return summary == null ? null : "Ok";
                });
            }
            finally
            {
                try
                {
                    if (File.Exists(fileFull)) File.Delete(fileFull);
                }
                catch
                {
                }
            }

            return isSupported != null;
        }
    }
}