using System;

namespace Universe.Dashboard.DAL.Tests
{
    public class DbTestParameter
    {
        public EF.Family Family;
        public Func<DashboardContext> GetDashboardContext;
        public string ShortVersion;
        
        public override string ToString()
        {
            return $"{Family.ToString()}-v{ShortVersion}";
        }
    }
}