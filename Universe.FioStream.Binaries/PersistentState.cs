using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Universe.FioStream.Binaries
{
    public class PersistentState
    {

        private static readonly string MigrationVersion = "v1";

        private static HashSet<string> Nulls = new HashSet<string>();

        public static T GetOrStore<T>(string key, Func<T> getValue)
        {
            if (Nulls.Contains(key)) return default(T);

            var nameOnly = key;
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows || CrossInfo.ThePlatform == CrossInfo.Platform.MacOSX) 
                nameOnly = nameOnly.Replace(":\\", "→").Replace(":", "→").Replace(Path.DirectorySeparatorChar.ToString(), "→").Replace(Path.AltDirectorySeparatorChar.ToString(), "→");
            else
                nameOnly = nameOnly.Replace(Path.DirectorySeparatorChar.ToString(), "-").Replace(Path.AltDirectorySeparatorChar.ToString(), "-");
            
            var file = Path.Combine(StateFolder, MigrationVersion + "." + nameOnly);
            string rawText = null;
            if (File.Exists(file))
            {
                using(FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader rdr = new StreamReader(fs, Utf8))
                {
                    rawText = rdr.ReadToEnd();
                }
            }

            if (rawText != null)
            {
                if (typeof(T) == typeof(bool)) return (T) (object) rawText.Equals("True", StringComparison.OrdinalIgnoreCase);
                if (typeof(T) == typeof(string)) return (T) (object) rawText;
                if (typeof(T) == typeof(string[])) return (T) (object) ParseStrings(rawText);
                throw new NotSupportedException($"Type {typeof(T)} is not supported");
            }

            T ret = getValue();

            if (ret == null)
            {
                Nulls.Add(key);
                return ret;
            }
            
            rawText = (typeof(T) == typeof(string[]))
                ? SerializeStrings((string[])(object)ret)
                : Convert.ToString(ret);

            using(FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(fs, Utf8))
            {
                wr.Write(rawText);
            }

            return ret;
        }

        static string SerializeStrings(string[] arg) => $"{arg.Length}{Environment.NewLine}{string.Join("\n", arg)}";

        static string[] ParseStrings(string raw)
        {
            string[] arr = raw.Split('\n');
            if (int.TryParse(arr[0], out var len))
            {
                return len == 0 ? new string[0] : arr.Skip(1).Take(len).ToArray();
            }

            throw new ArgumentException($"Wrong serialized array '{raw}'", nameof(raw));
        }

        
        public static string TempFolder => _TempFolder.Value;
        public static string BinFolder => _BinFolder.Value;
        public static string StateFolder => _StateFolder.Value;
        
        private static Lazy<string> _TempFolder = new Lazy<string>(() =>
        {
            return GetDir($".local{Path.DirectorySeparatorChar}tmp");
        });

        private static Lazy<string> _BinFolder = new Lazy<string>(() =>
        {
            return GetDir($".local{Path.DirectorySeparatorChar}bin");
        });

        private static Lazy<string> _StateFolder = new Lazy<string>(() =>
        {
            return GetDir($".local{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}state");
        });

        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

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
                if (!string.IsNullOrEmpty(candidate) && Directory.Exists(candidate))
                {
                    var fullPath = Path.Combine(candidate, subFolder);
                    try
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    catch
                    {
                    }

                    if (Directory.Exists(fullPath))
                    {
                        // Console.WriteLine($"SUBFOLDER: {fullPath}");
                        return fullPath;
                    }
                }
            }

            throw new Exception($"Unable to create local directory {subFolder}");
        }
        
    }
}