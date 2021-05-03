﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestCases
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding enc = new UTF8Encoding(false);
            Console.WriteLine($"Dir: ${Environment.CurrentDirectory}");
            var files = new DirectoryInfo(".").GetFiles("fio-*.benchmark.txt");
            // files = files.OrderBy(x => x.Name, comparer: StringComparer.InvariantCultureIgnoreCase).ToArray();
            List<string> csVersions = new List<string>();
            foreach (var fileInfo in files)
            {
                var arrVer = fileInfo.Name.Split('-');
                var ver = arrVer.Length > 1 ? arrVer[1] : "unknown";
                List<List<string>> sections = new List<List<string>>();
                using(FileStream fs= new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader rdr = new StreamReader(fs, enc))
                {
                    var fileContent = rdr.ReadToEnd();
                    var lines = fileContent.Split((char) 10, (char)13);
                    List<string> section = new List<string>();
                    foreach (var line in lines)
                    {
                        bool isSplitter = line != "" && line == new string('-', line.Length);
                        if (isSplitter)
                        {
                            sections.Add(section);
                            section = new List<string>();
                        }
                        else
                        {
                            section.Add(line);
                        }
                    }
                    if (section.Count > 0) sections.Add(section);

                    Console.WriteLine($"File: {fileInfo.Name}, Lines: {lines.Length}, Sections: {sections.Count}");
                    csVersions.Add(TestWriter.Generate1(ver, sections));
                }

                using(FileStream fs = new FileStream("AutoGeneratedTests.FioParserTestCase.cs", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter wr = new StreamWriter(fs, enc))
                {
                    wr.WriteLine(TestWriter.GenerateFull(csVersions));
                }
            }

        }
    }
}