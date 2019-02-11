﻿using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MountLab
{
    static class DebugDumper
    {
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

            // string json = JsonConvert.SerializeObject(anObject, Formatting.Indented, settings);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                wr.Write(json);
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

        public static void Trace(string info)
        {
            using (FileStream dump = new FileStream("app.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(dump, new UTF8Encoding(false)))
            {
                wr.WriteLine(DateTime.Now.ToString("yyyy MM dd HH:mm:ss") + " " + info);
            }
        }
    }
}