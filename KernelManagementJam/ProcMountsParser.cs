using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace LinuxNetStatLab
{

    public class MountEntry
    {
        // May be a symbolic link to /dev/zzz
        public string Device { get; set; }
        // May be multiple
        public string MountPath { get; set; }

        public string FileSystem { get; set; }

        public override string ToString()
        {
            return $"{nameof(Device)}: {Device}, {nameof(MountPath)}: {MountPath}, {nameof(FileSystem)}: {FileSystem}";
        }
    }


    /*
     * 0: filesystem|/dev/device_plus_partition
     * 1: mount-point
     * 2: file-system-type
     * 3: options
     * 4: priority
     * 5: something else
     */

    /*
     * Special characters encoded as \040
     */
    public class ProcMountsParser
    {
        static readonly UTF8Encoding FileEncoding = new UTF8Encoding(false);
        public IList<MountEntry> Entries { get; private set; }

        public static ProcMountsParser Parse()
        {
            return Parse("/proc/mounts");
        }

        public static ProcMountsParser Parse(string fakeMountsFilePath)
        {
            ProcMountsParser ret = new ProcMountsParser();
            ret.ParseImpl(fakeMountsFilePath);
            return ret;
        }

        private void ParseImpl(string fileName)
        {
            Entries = new List<MountEntry>();
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rdr = new StreamReader(fs, FileEncoding))
            {
                var content = rdr.ReadToEnd();
                StringReader lines = new StringReader(content);
                string line = null;
                do
                {
                    line = lines.ReadLine();
                    if (line == null) break;
                    var columns = SpaceSeparatedDecoder.DecodeIntoColumns(line);
                    if (columns.Length >= 4)
                    {
                        Entries.Add(new MountEntry()
                        {
                            Device = columns[0],
                            MountPath = columns[1],
                            FileSystem = columns[2],
                        });
                    }
                } while (line != null);
            }
        }
    }

    public static class SpaceSeparatedDecoder
    {
        public static string[] DecodeIntoColumns(string rawLine)
        {
            List<string> ret = new List<string>();
            var arr = rawLine.Split(' ');
            foreach (var s in arr)
            {
                ret.Add(DecodeSpecialChars(s));
            }

            return ret.ToArray();
        }

        public static string DecodeSpecialChars(string encodedString)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < encodedString.Length; i++)
            {
                if (encodedString[i] == '\\')
                {
                    bool isOk = false;
                    if (i + 1 < encodedString.Length && encodedString[i + 1] == '\\')
                    {
                        // Ubuntu 18 in docker on macOS
                        isOk = true;
                        ret.Append('\\');
                        i++;
                    }
                    else if (i + 3 < encodedString.Length)
                    {
                        string part = encodedString.Substring(i + 1, 3);
                        int octal;
                        bool isOctal = Int32.TryParse(part, NumberStyles.Integer, new CultureInfo("en-US"), out octal);
                        if (isOctal)
                        {
                            int integer = Convert.ToInt32(octal.ToString("0"), 8);
                            isOk = true;
                            ret.Append((char) integer);
                        }
                        i += 3;
                    }

                    if (!isOk)
                        Trace.WriteLine(string.Format("Improperly encoded string: [{0}]", encodedString));

                }
                else
                    ret.Append(encodedString[i]);
            }

            return ret.ToString();
        }
    }
}
