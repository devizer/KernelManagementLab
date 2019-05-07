using Newtonsoft.Json.Linq;

namespace Universe.Dashboard.Agent
{
    // Internet might be absent
    public class NewVersionDataSource
    {
        private const string InitialValue = "{'Kind': 'Data is not yet ready'}";
        static JObject _NewVersion = JObject.Parse(InitialValue);
        static readonly object Sync = new object();

        public static JObject NewVersion
        {
            get
            {
                lock (Sync) return _NewVersion;
            }
            internal set
            {
                lock (Sync) _NewVersion = value;
            }
        }

            
    }
}