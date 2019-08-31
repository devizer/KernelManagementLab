using System;
using Universe.Dashboard.DAL.MultiProvider;

namespace Universe.Dashboard.DAL.Tests
{
    public class TestDatabase
    {
        public EF.Family Family;
        public string ShortVersion;
        public Func<DashboardContext> GetDashboardContext;
        
        public override string ToString()
        {
            return $"{Family.ToString()}-v{ShortVersion}";
        }
    }
}