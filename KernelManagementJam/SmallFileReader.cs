using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using KernelManagementJam.DebugUtils;

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
            if (!File.Exists(fileName))
            {
                AppendSingleLinerLog(() => string.Format("[{0}] single-line file not found", fileName));
                return null;
            }

            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var rdr = new StreamReader(fs, FileEncoding))
                {
                    var ret = rdr.ReadLine();

                    AppendSingleLinerLog(() => string.Format("[{0}] first line: '{1}'", fileName, ret));

                    return ret;
                }
            }
            catch (DirectoryNotFoundException)
            {
                AppendSingleLinerLog(() => string.Format("[{0}] single-line file not found", fileName));
                return null;
            }
            catch (FileNotFoundException)
            {
                // a copypaste
                AppendSingleLinerLog(() => string.Format("[{0}] single-line file not found", fileName));
                return null;
            }
            catch (IOException)
            {
                // a copypaste
                AppendSingleLinerLog(() => string.Format("System.IO.IOException for [{0}]", fileName));
                return null;
            }
        }

        [Conditional("DEBUG")]
        private static void AppendSingleLinerLog(Func<string> logLine)
        {
            if (!DebugDumper.AreDumpsEnabled) return;

            var fullFileName = Path.Combine(DebugDumper.DumpDir, "SmallFileReader::One-Line-Reader.log");
            // CheckDir(fullFileName);

            using (var dump = new FileStream(fullFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var wr = new StreamWriter(dump, FileEncoding))
            {
                wr.WriteLine(logLine());
            }
        }
    }
}
