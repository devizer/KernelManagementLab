using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace KernelManagementJam.DebugUtils
{
    public static class DebugDumper
    {
        [Conditional("DEBUG")]
        public static void Dump(object anObject, string fileName)
        {
            JsonSerializer ser = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            StringBuilder json = new StringBuilder();
            StringWriter jwr = new StringWriter(json);
            ser.Serialize(jwr, anObject);
            jwr.Flush();

            CheckDir(fileName);

            // string json = JsonConvert.SerializeObject(anObject, Formatting.Indented, settings);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                wr.Write(json);
            }
        }

        public static void DumpText(string content, string fileName)
        {
            CheckDir(fileName);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
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

        [Conditional("DEBUG")]
        public static void Trace(string info)
        {
            var fileName = "debug-dumps/app.log";
            CheckDir(fileName);
            using (FileStream dump = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(dump, new UTF8Encoding(false)))
            {
                wr.WriteLine(DateTime.Now.ToString("yyyy MM dd HH:mm:ss") + " " + info);
            }
        }
        
        private static void CheckDir(string fileName)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            catch
            {
            }
        }


    }
}