using System;
using System.IO;
using System.Threading;

namespace Universe.Dashboard.DAL
{
    class GarbageCollector4Sqlite
    {
        public static void CleanUpPrevVersions(string fullNameOfCurrentVersion)
        {
            // run it in foreground
            Thread t = new Thread(() => {
                TryAndForget(() =>
                {
                    var dir = Path.GetDirectoryName(fullNameOfCurrentVersion);
                    var ext = Path.GetExtension(fullNameOfCurrentVersion);
                    string[] files = Directory.GetFiles(dir, $"*{ext}");
                    foreach (var file in files)
                    {
                        if (file != fullNameOfCurrentVersion)
                        {
                            TryAndForget(() =>
                            {
                                File.Delete(file);
                                Console.WriteLine($"Deleted prev dashboard history: {file}");
                            });
                        }
                    }

                });
            });
            t.Start();
        }

        static void TryAndForget(Action a)
        {
            try
            {
                a();
            }
            catch
            {
            }
        }
        
    }
}