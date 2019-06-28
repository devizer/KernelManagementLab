using System;
using System.Diagnostics;
using System.Threading;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL
{
    public static class EFHealth
    {
        public static Exception WaitFor(this DbContext db, int timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Exception ret = null;
            do
            {
                try
                {
                    db.Database.GetDbConnection().Execute("Select null;");
                    return null;
                }
                catch (Exception ex)
                {
                    ret = ex;
                    if (sw.ElapsedMilliseconds > timeout) return ret;
                    Thread.Sleep(200);
                }

            } while (sw.ElapsedMilliseconds < timeout);

            return ret;
        }
    }
}