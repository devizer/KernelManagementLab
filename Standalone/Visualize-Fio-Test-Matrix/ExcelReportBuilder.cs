using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace VisualizeFioTestMatrix
{
    // TODO: Manually test armv7 and aarch64 with libaio
    class ExcelReportBuilder
    {
        static Color HeaderColor = Color.Black;
        static Color DataColor = Color.FromArgb(255, 170, 170, 170);
        private static readonly string SheetName = "TLS Report";

        public DataSource DataSource { get; }

        public ExcelReportBuilder(DataSource dataSource)
        {
            DataSource = dataSource;
        }

        public void Build(string excelFileName)
        {
            var file = new FileInfo(excelFileName);
            using (var package = new ExcelPackage(file))
            {
                
                foreach (var archName in DataSource.AllArchs)
                {
                    ExcelWorksheet archSheet = package.Workbook.Worksheets.GetOrAddByName(archName);

                    var archBenchmarks = DataSource.RawBenchmarkList.Where(x =>x.Arch == archName).ToArray();
                    var archEngines = archBenchmarks.Select(x => x.Engine).Distinct().OrderBy(x => x).ToArray();

                    string GetOsByImage(string argImage)
                    {
                        return archBenchmarks.FirstOrDefault(x => x.Image == argImage)?.OsAndVersion;
                    }
                    var archImages = archBenchmarks
                        .Select(x => x.Image)
                        .Distinct().OrderBy(x => GetOsByImage(x)).ToList();

                    // COL1 - OS, COL2 - gLibc ver, COL3 - Image  
                    int iImage = 0;
                    foreach (var image in archImages)
                    {
                        var osAndVersion = archBenchmarks.FirstOrDefault(x => x.Image == image)?.OsAndVersion;
                        var gLibCVersionVersion = archBenchmarks.FirstOrDefault(x => x.Image == image)?.GLibCVersion;
                             
                        var cellHeaderOs = archSheet.Cells[3 + iImage, 1];
                        cellHeaderOs.Value = osAndVersion;
                        
                        var cellHeaderGLibVersion = archSheet.Cells[3 + iImage, 2];
                        cellHeaderGLibVersion.Value = gLibCVersionVersion;
                        cellHeaderGLibVersion.IgnoreNumberStoredAsText();
                        
                        var cellHeaderImage = archSheet.Cells[3 + iImage, 3];
                        cellHeaderImage.Value = image;
                        cellHeaderImage.Style.WrapText = false;

                        // TOTAL for engines
                        int iEngine = 0;
                        foreach (var engine in archEngines)
                        {
                            var total = archBenchmarks.Where(x => x.IsSuccess && x.Engine == engine && x.Image == image).Count();
                            var headerFioX = 4;
                            if (iImage == 0) archSheet.Cells[2, headerFioX + iEngine].Value = engine;
                            var headerEngineCell = archSheet.Cells[3+iImage, headerFioX + iEngine];
                            headerEngineCell.Value = total;
                            headerEngineCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            headerEngineCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            headerEngineCell.IgnoreNumberStoredAsText();
                            archSheet.Columns[headerFioX + iEngine].Width = 12;
                            iEngine++;
                        }

                        
                        iImage++;
                    }
                    archSheet.Columns[1].AutoFit(7);
                    archSheet.Columns[2].Width = 7;
                    archSheet.Columns[2].Style.WrapText = false;
                    archSheet.Columns[3].Width = 10;
                    
                    // Header FIO*Engine
                    var fioRawList = archBenchmarks.Select(x => x.FioRaw).Distinct().OrderByDescending(x => x).ToArray();
                    int iFio = 0;
                    foreach (var fioRaw in fioRawList)
                    {
                        int headerFioX = 4 + (iFio+1) * archEngines.Length;
                        var headerFioCell = archSheet.Cells[1, headerFioX, 1, headerFioX + archEngines.Length - 1];
                        headerFioCell.Value = FioHeaderFormatter.Format(fioRaw);
                        headerFioCell.Style.WrapText = true;
                        // headerFioCell.Style.TextRotation = 90;
                        headerFioCell.Merge = true;
                        headerFioCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        headerFioCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        int iEngine = 0;
                        foreach (var engine in archEngines)
                        {
                            var headerEngineCell = archSheet.Cells[2, headerFioX + iEngine];
                            headerEngineCell.Value = engine.Substring(0,3);
                            headerEngineCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            headerEngineCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            archSheet.Columns[headerFioX + iEngine].Width = 5;
                            iEngine++;
                        }
                        

                        iFio++;
                    }
                    
                    // DATA Cells
                    iImage = 0;
                    foreach (var image in archImages)
                    {
                        var archImageBenchmarks = archBenchmarks.Where(x => x.Image == image).ToArray();
                        iFio = 0;
                        foreach (var fioRaw in fioRawList)
                        {
                            int headerFioX = 4 + (iFio+1) * archEngines.Length;
                            int iEngine = 0;
                            foreach (var engine in archEngines)
                            {
                                var tempBenchmarks = archImageBenchmarks.Where(x => x.FioRaw == fioRaw && x.Engine == engine).ToArray();
                                if (tempBenchmarks.Length > 1)
                                    throw new InvalidOperationException("Found more then one by fio+image+engine+arch");
                                
                                RawBenchmark benchmark = tempBenchmarks.FirstOrDefault();
                                
                                var dataCell = archSheet.Cells[3+iImage, headerFioX + iEngine];
                                dataCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                dataCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                if (benchmark != null && benchmark.IsSuccess)
                                {
                                    dataCell.Value = " ✔️"; // ☑ ✅
                                    dataCell.Style.Font.Color.SetColor(Color.DarkGreen);
                                }
                                else if (benchmark != null)
                                {
                                    dataCell.Value = "-"; // ⚠ ❗ 🤕 no way 💩 🤬😡🥵🤕👺👹💔❗
                                    dataCell.Style.Font.Color.SetColor(Color.DarkRed);
                                    dataCell.Style.Font.Bold = true;
                                }

                                iEngine++;
                            }

                            iFio++;
                        }

                        iImage++;
                    }


                    string[] topLeftHeaders = new[] { "OS", "libc", "Image" };
                    for (int c = 0; c < topLeftHeaders.Length; c++)
                    {
                        var cellTopLeft = archSheet.Cells[1, 1 + c, 2, 1 + c];
                        cellTopLeft.Value = topLeftHeaders[c];
                        cellTopLeft.Merge = true;
                        cellTopLeft.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellTopLeft.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    
                    archSheet.Rows[2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    archSheet.Rows[1].Height = 66;
                    archSheet.View.FreezePanes(3, 3);
                    

                }
                
                package.Save();
            }
        }
    }
}
