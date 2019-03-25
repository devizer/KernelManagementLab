using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Universe.Dashboard.DAL
{
    public class HistoryLogic
    {
        private DashboardContext _DbContext;

        public HistoryLogic(DashboardContext dbContext)
        {
            _DbContext = dbContext;
        }

        public bool TryLoad<T>(string key, out T value)
        {
            var entity = _DbContext.HistoryCopy.FirstOrDefault(x => x.Key == key);
            if (entity == null || entity.JsonBlob == null)
            {
                value = default(T);
                return false;
            }

            value = ParseJson<T>(entity.JsonBlob);
            return true;
        }
        
        public void Save<T>(string key, T value) where T: class
        {
            
            var entity = _DbContext.HistoryCopy.FirstOrDefault(x => x.Key == key);
            if (value == default(T))
            {
                if (entity != null)
                    _DbContext.HistoryCopy.Remove(entity);
                else
                    return;
            }
            else
            {
                var json = AsJson(value);
                if (entity == null)
                {
                    entity = new HistoryCopy() {Key = key, JsonBlob = AsJson(value)};
                    _DbContext.HistoryCopy.Add(entity);
                }
                else
                {
                    entity.JsonBlob = json;
                }
            }

            _DbContext.SaveChanges();
        }
        
        static string AsJson(object arg)
        {
            JsonSerializer ser = new JsonSerializer()
            {
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            StringBuilder json = new StringBuilder();
            StringWriter jwr = new StringWriter(json);
            ser.Serialize(jwr, arg);
            jwr.Flush();

            return json.ToString();
        }

        static T ParseJson<T>(string asJson)
        {
            JsonSerializer ser = new JsonSerializer()
            {
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            JsonTextReader jsonReader = new JsonTextReader(new StringReader(asJson));
            return ser.Deserialize<T>(jsonReader);
        }

    }
}