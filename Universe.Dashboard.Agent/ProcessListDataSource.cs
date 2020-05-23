using System.Collections.Generic;

namespace Universe.Dashboard.Agent
{
    public class ProcessListDataSource
    {
        static List<AdvancedProcessStatPoint> _Processes = new List<AdvancedProcessStatPoint>();
        private static int _ProcessListUpdateInterval = 5000;
        private static readonly object Sync = new object();
        

        public static List<AdvancedProcessStatPoint> Processes
        {
            get
            { 
                lock (Sync) return _Processes;
            } 
            set
            {
                lock(Sync) _Processes = value;
            }
        }

        public static int ProcessListUpdateInterval
        {
            get
            {
                lock(Sync) return _ProcessListUpdateInterval;
            }
            set
            {
                lock(Sync) _ProcessListUpdateInterval = value;
            }
        }
    }
}