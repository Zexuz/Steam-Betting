using Newtonsoft.Json;

namespace Shared.Shared.Web
{
    public class Parser<T>
    {
        public static T FromJson(string json) => JsonConvert.DeserializeObject<T>(json, Converter.Settings);
    }
}