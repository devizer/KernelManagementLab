﻿using System;
using System.Diagnostics;
using System.IO;

class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");
        ExtractRawSources();
    }

    static void ExtractRawSources()
    {
        if (!Directory.Exists("structured-fio-benchmark-results "))
        {
            Console.WriteLine("Extract structured-fio-benchmark-results.7z");
            ProcessStartInfo si = new ProcessStartInfo("7z", "x -y structured-fio-benchmark-results.7z");
            using (Process p = Process.Start(si))
            {
                p.WaitForExit();
            }
        }
    }
}
