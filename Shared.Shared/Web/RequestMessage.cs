using System.Collections.Generic;
using System.Net.Http;

namespace Shared.Shared.Web
{
    public class RequestMessage
    {
        public HttpMethod                       Method  { get; }
        public string                           Url     { get; }
        public Dictionary<string, List<string>> Headers { get; set; }
        public Dictionary<string, string>       Body    { get; set; }

        public RequestMessage
        (
            HttpMethod method,
            string url,
            Dictionary<string, List<string>> headers = null,
            Dictionary<string, string> body = null
        )
        {
            Method = method;
            Url = url;
            Headers = headers;
            Body = body;
        }
    }
}