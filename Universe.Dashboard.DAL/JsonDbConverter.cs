using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Universe.Dashboard.DAL
{
    public class JsonDbConverter
    {
        public static ValueConverter<T, string> Create<T>()
        {
            return new ValueConverter<T, string>(
                value => AsJson(value),
                json => ParseJson<T>(json)
            );
        }

        public static string AsJson(object arg)
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

        public static T ParseJson<T>(string asJson)
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