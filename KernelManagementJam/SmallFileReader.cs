using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KernelManagementJam
{
    class SmallFileReader
    {
        static readonly UTF8Encoding FileEncoding = new UTF8Encoding(false);

        public static IEnumerable<string> ReadLines(string fileName)
        {
            string content;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rdr = new StreamReader(fs, FileEncoding))
            {
                content = rdr.ReadToEnd();
            }

            StringReader lines = new StringReader(content);
            string line = lines.ReadLine();
            while (line != null)
            {
                yield return line;
                line = lines.ReadLine();
            }
        }

        public static string ReadFirstLine(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rdr = new StreamReader(fs, FileEncoding))
            {
                var ret = rdr.ReadLine();

                using (FileStream dump = new FileStream("One-Line-Reader.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter wr = new StreamWriter(dump, FileEncoding))
                    wr.WriteLine("[{0}] first line: '{1}'", fileName, ret);

                return ret;
            }
        }
    }
}