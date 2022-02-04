
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace VisualizeFioTestMatrix
{
    // Manually test armv7 and aarch64 with libaio
    class ManualFioTest
    {
        public DataSource DataSource { get; }

        public ManualFioTest(DataSource dataSource)
        {
            DataSource = dataSource;
        }

        public void Build()
        {
            var archs = DataSource.AllArchs;
            var srcFioManualTest = File.ReadAllText("Fio-Manual-Test.sh");
            foreach (var arch in archs)
            {
                var fileName = $"Manual-Fio-Test-{arch}.sh";
                using(FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter wr = new StreamWriter(fs, new UTF8Encoding(false)))
                {
                    wr.WriteLine(srcFioManualTest);
                    var archBenchmarks = DataSource.RawBenchmarkList.Where(x => x.Arch == arch).ToArray();
                    var allFio = archBenchmarks.Select(x => x.FioRaw).Distinct().OrderByDescending(x => x).ToArray();
                    wr.WriteLine($"FIO_TOTAL={allFio.Length}");

                    foreach (var fioRaw in allFio)
                    {
                        bool isVer3 = fioRaw.StartsWith("fio=");
                        string url = isVer3
                            ? $"https://master.dl.sourceforge.net/project/fio/ver-3/{UrlEncoder.Create(UnicodeRanges.All).Encode(fioRaw)}/fio.tar.xz?viasf=1"
                            : $"https://master.dl.sourceforge.net/project/fio/ver2/{UrlEncoder.Create(UnicodeRanges.All).Encode(fioRaw)}.xz?viasf=1";

                        var caption = FioHeaderFormatter.Format(fioRaw).Replace(Environment.NewLine, " ");
                        caption = caption.Replace(" ", " ");
                        
                        wr.WriteLine($@"Fio-Manual-Test {(isVer3 ? "VER-3" : "VER-2")} {("'" + caption + "'"),-37} ""{url}"" ");
                    }
                }


            }
        }
    }
}
