using System;
using System.Diagnostics;
using System.IO;
using MySql.Data.MySqlClient;

namespace Universe.Dashboard.DAL.Tests
{
    class MySqlDumper
    {
        public static void Dump(string connectionString, string fileName)
        {
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder(connectionString);
            // MYSQL_PWD=\"$MYSQL_ROOT_PASSWORD\" mysqldump --protocol=TCP -h localhost -u root -P 3306 mysql
            ProcessStartInfo si = new ProcessStartInfo("mysqldump", $"--protocol=TCP -h \"{b.Server}\" -u \"{b.UserID}\" -P {b.Port} \"{b.Database}\"");
            si.Environment["MYSQL_PWD"] = b.Password;
            si.RedirectStandardOutput = true;
            string content;
            using (var p = Process.Start(si))
            {
                p.WaitForExit();
                content = p.StandardOutput.ReadToEnd();
                if (p.ExitCode != 0)
                    throw new Exception($"Unable to dump MySQL DB. Exit Code is {p.ExitCode}");
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            catch
            {
            }
            
            File.WriteAllText(fileName, content);
        }
    }
}