using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace VisualizeFioTestMatrix
{
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

                        archSheet.Cells[3 + iImage, 4].Value = ((char)160).ToString();
                        
                        iImage++;
                    }
                    archSheet.Columns[1].AutoFit(7);
                    archSheet.Columns[2].Width = 7;
                    archSheet.Columns[2].Style.WrapText = false;
                    archSheet.Columns[3].Width = 10;

                    
                    var fioRawList = archBenchmarks.Select(x => x.FioRaw).Distinct().OrderBy(x => x).ToArray();
                    var archEngines = archBenchmarks.Select(x => x.Engine).Distinct().OrderBy(x => x).ToArray();
                    int iFio = 0;
                    foreach (var fioRaw in fioRawList)
                    {
                        int headerFioX = 4 + (iFio * archEngines.Length);
                        var headerFioCell = archSheet.Cells[1, headerFioX, 1, headerFioX + archEngines.Length - 1];
                        headerFioCell.Style.TextRotation = 90;
                        headerFioCell.Merge = true;
                        headerFioCell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                        headerFioCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        int iEngine = 0;
                        foreach (var engine in archEngines)
                        {
                            var headerEngineCell = archSheet.Cells[2, headerFioX + iEngine];
                            headerEngineCell.Value = engine;
                            archSheet.Columns[headerFioX + iEngine].Width = 3;
                            iEngine++;
                        }
                        
                        headerFioCell.Value = fioRaw;

                        iFio++;
                    }

                    archSheet.Rows[1].Height = 100;
                }
                package.Save();
            }
        }
    }
}
