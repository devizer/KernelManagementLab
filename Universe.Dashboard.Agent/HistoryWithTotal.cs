using System.Collections.ObjectModel;

namespace Universe.Dashboard.Agent
{
    public class HistoryWithTotal<T> : Collection<PointWithTotal<T>>
    {
    }
    
    public class PointWithTotal<T>
    {
        public T Total { get; set; }
        public T Current { get; set; }
    }

}