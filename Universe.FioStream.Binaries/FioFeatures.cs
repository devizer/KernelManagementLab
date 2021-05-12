using System;

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
                        engines = rdr.GetEngileList();
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
    }
}