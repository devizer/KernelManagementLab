using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace KernelManagementJam
{
    internal class SmallFileReader
    {
        private static readonly UTF8Encoding FileEncoding = new UTF8Encoding(false);

        public static IEnumerable<string> ReadLines(string fileName)
        {
            string content;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var rdr = new StreamReader(fs, FileEncoding))
            {
                content = rdr.ReadToEnd();
            }

            var lines = new StringReader(content);
            var line = lines.ReadLine();
            while (line != null)
            {
                yield return line;
                line = lines.ReadLine();
            }
        }

        public static string ReadFirstLine(string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var rdr = new StreamReader(fs, FileEncoding))
                {
                    var ret = rdr.ReadLine();
                    return ret;
                }
            }
            catch (Exception)
            {
                return null;
            }

            if (!File.Exists(fileName))
            {
                AppendSingleLinerLog(string.Format("[{0}] single-line file not found", fileName));
                return null;
            }

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var rdr = new StreamReader(fs, FileEncoding))
            {
                var ret = rdr.ReadLine();

                var logLine = string.Format("[{0}] first line: '{1}'", fileName, ret);
                AppendSingleLinerLog(logLine);

                return ret;
            }
        }

        public static string ReadFirstLine_Slow(string fileName)
        {
            if (!File.Exists(fileName))
            {
                AppendSingleLinerLog(string.Format("[{0}] single-line file not found", fileName));
                return null;
            }

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var rdr = new StreamReader(fs, FileEncoding))
            {
                var ret = rdr.ReadLine();

                var logLine = string.Format("[{0}] first line: '{1}'", fileName, ret);
                AppendSingleLinerLog(logLine);

                return ret;
            }
        }

        [Conditional("DEBUG")]
        private static void AppendSingleLinerLog(string logLine)
        {
            using (var dump = new FileStream("One-Line-Reader.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var wr = new StreamWriter(dump, FileEncoding))
            {
                wr.WriteLine(logLine);
            }
        }
    }
}