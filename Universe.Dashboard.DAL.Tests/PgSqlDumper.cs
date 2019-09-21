using System;
using System.Diagnostics;
using System.IO;
using Npgsql;

namespace Universe.Dashboard.DAL.Tests
{
    class PgSqlDumper
    {
        // PGPASSWORD=pass psql -t -h localhost -p 5432 -U w3top -q -c "select 'Hello, ' || current_user;" -d w3top
        
        public static void Dump(string connectionString, string fileName)
        {
            NpgsqlConnectionStringBuilder b = new NpgsqlConnectionStringBuilder(connectionString);
            ProcessStartInfo si = new ProcessStartInfo("pg_dump", $"--inserts -h \"{b.Host}\" -U \"{b.Username}\" -p {b.Port} \"{b.Database}\"");
            si.Environment["PGPASSWORD"] = b.Password;
            si.RedirectStandardOutput = true;
            string content;
            using (var p = Process.Start(si))
            {
                p.WaitForExit();
                content = p.StandardOutput.ReadToEnd();
                if (p.ExitCode != 0)
                    throw new Exception($"Unable to dump PgSql DB. Exit Code is {p.ExitCode}");
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