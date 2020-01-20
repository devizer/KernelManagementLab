using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KernelManagementJam.DebugUtils
{
    public static class DebugDumper
    {
        public static string DumpDir => _GetDumpDir.Value;

        public static bool AreDumpsEnabled => _DumpsMode.Value != DumpsMode.Off;

        enum DumpsMode
        {
            On,
            SideBySide,
            Off
        }

        private static Lazy<DumpsMode> _DumpsMode = new Lazy<DumpsMode>(() =>
        {
            var raw = Environment.GetEnvironmentVariable("DUMPS_ARE_ENABLED");
            string[] yes = new[] {"On", "True", "1"};
            var isProd = yes.Any(x => x.Equals(raw, StringComparison.InvariantCultureIgnoreCase));
            if (isProd) return DumpsMode.On;
            if ("SideBySide".Equals(raw, StringComparison.InvariantCultureIgnoreCase))
                return DumpsMode.SideBySide;

            return DumpsMode.Off;
        }, LazyThreadSafetyMode.ExecutionAndPublication);
        
        // [Conditional("DUMPS")]
        public static void Dump(object anObject, string fileName, bool minify = false)
        {
            if (!AreDumpsEnabled) return;
            
            var profilerPath = new AdvancedMiniProfilerKeyPath("DUMPS", fileName, minify ? "minified": "intended");
            using (AdvancedMiniProfiler.Step(profilerPath))
            {

                JsonSerializer ser = new JsonSerializer()
                {
                    Formatting = !minify ? Formatting.Indented : Formatting.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };

                ser.ContractResolver = TheContractResolver;

                StringBuilder json = new StringBuilder();
                StringWriter jwr = new StringWriter(json);
                ser.Serialize(jwr, anObject);
                jwr.Flush();

                var fullFileName = Path.Combine(DumpDir, fileName);
                CheckDir(fullFileName);

                // string json = JsonConvert.SerializeObject(anObject, Formatting.Indented, settings);
                using (FileStream fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write,
                    FileShare.ReadWrite))
                    
                using (StreamWriter wr = new StreamWriter(fs, new UTF8Encoding(false)))
                {
                    wr.Write(json);
                }
            }
        }

        public static void DumpText(string content, string fileName)
        {
            if (!AreDumpsEnabled) return;

            var fullFileName = Path.Combine(DumpDir, fileName);
            CheckDir(fullFileName);

            using (FileStream fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                wr.Write(content);
            }
        }

        public static string AsJson(this object arg)
        {
            JsonSerializer ser = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            StringBuilder json = new StringBuilder();
            StringWriter jwr = new StringWriter(json);
            ser.Serialize(jwr, arg);
            jwr.Flush();

            return json.ToString();
        }

        [Conditional("DUMPS")]
        public static void Trace(string info)
        {
            if (!AreDumpsEnabled) return;
            var fileName = "app.trace.log";
            var fullFileName = Path.Combine(DumpDir, fileName);
            CheckDir(fullFileName);

            using (FileStream dump = new FileStream(fullFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(dump, new UTF8Encoding(false)))
            {
                wr.WriteLine(DateTime.Now.ToString("yyyy MM dd HH:mm:ss") + " " + info);
            }
        }
        
        private static void CheckDir(string fileName)
        {
            if (true || Path.GetDirectoryName(fileName) != DumpDir)
            {
                // Console.WriteLine($"Checking suspicious directory of file [{fileName}]");
                if (Directory.Exists(Path.GetDirectoryName(fileName))) return;
                
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }
                catch
                {
                }
            }
        }

        private static Lazy<string> _GetDumpDir = new Lazy<string>(() =>
        {
            var an = Path.GetFileName(Assembly.GetEntryAssembly().Location);
            if (_DumpsMode.Value == DumpsMode.SideBySide) an += $"({Assembly.GetEntryAssembly().GetName().Version})";
            var ret = "DUMPS-" + an;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                ret = Path.Combine("/tmp", ret);
            }
            else
            {
                ret = Path.Combine("bin", ret);
            }
            
            ret = new DirectoryInfo(ret).FullName;
            Console.WriteLine("DUMPS folder: [" + ret + "]");
            if (!Directory.Exists(ret))
                Directory.CreateDirectory(ret);
            
            return ret;
        });

        private static readonly DefaultContractResolver TheContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                OverrideSpecifiedNames = false,
                ProcessDictionaryKeys = true,
            }
        };
    }
}
