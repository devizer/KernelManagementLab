using Universe.Dashboard.DAL.MultiProvider;

namespace Universe.Dashboard.DAL
{
    public class DashboardContextRuntimeParameters
    {
        public EF.Family Family { get; set; }
        public string ConnectionString { get; set; }
    }
}