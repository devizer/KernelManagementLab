using System.Linq;

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

            value = JsonDbConverter.ParseJson<T>(entity.JsonBlob);
            return true;
        }
        
        // Metrics are usually saved in single threaded mode 
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
                var json = JsonDbConverter.AsJson(value);
                if (entity == null)
                {
                    entity = new HistoryCopyEntity() {Key = key, JsonBlob = json};
                    _DbContext.HistoryCopy.Add(entity);
                }
                else
                {
                    entity.JsonBlob = json;
                }
            }
            
            DbResilience.ExecuteWriting(
                $"Save {key} to History", 
                () => _DbContext.SaveChanges(),
                totalMilliseconds: 1000,
                retryCount: 9999999);
        }
        

    }
}