using System;
using System.Diagnostics;
using System.IO;

namespace VisualizeFioTestMatrix
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello, World!");
            ExtractRawSources();
            var rawBenchmarks = DataSourceReader.Read();
            DataSource dataSource = new DataSource(rawBenchmarks);

            ManualFioTest manualFioTest = new ManualFioTest(dataSource);
            manualFioTest.Build();
            

            ExcelReportBuilder xlBuilder = new ExcelReportBuilder(dataSource);
            var excelFile = @$"FIO-Matrix-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.xlsx";
            using (Log.Duration("Generate Excel Report"))
            {
                xlBuilder.Build(excelFile);
            }

            ProcessStartInfo si = new ProcessStartInfo(excelFile);
            si.UseShellExecute = true;
            Process.Start(si);
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
}
