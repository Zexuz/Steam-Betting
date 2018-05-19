using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Shared.Shared.Web
{
    public interface IJsonRequestParser
    {
        CookieContainer CookieContainer { get; }
        Task<JObject> ExecuteAsJObject(RequestMessage message);
        Task<T>       ExecuteAsType<T>(RequestMessage message);
        Task          ExecuteAsVoid(RequestMessage message);
    }
}