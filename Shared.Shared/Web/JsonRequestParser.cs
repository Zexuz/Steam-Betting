using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Shared.Shared.Web.Exceptions;

namespace Shared.Shared.Web
{
    public class JsonRequestParser : IJsonRequestParser
    {
        public CookieContainer CookieContainer { get; }

        private readonly HttpClient      _httpClient;

        public JsonRequestParser()
        {
            CookieContainer = new CookieContainer();
            _httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = CookieContainer
            });
        }


        public async Task<JObject> ExecuteAsJObject(RequestMessage message)
        {
            var jsonString = await Execute(message);
            return JObject.Parse(jsonString);
        }

        public async Task<T> ExecuteAsType<T>(RequestMessage message)
        {
            var jsonString = await Execute(message);
            return Parser<T>.FromJson(jsonString);
        }

        public async Task ExecuteAsVoid(RequestMessage message)
        {
            await Execute(message);
        }

        private async Task<string> Execute(RequestMessage message)
        {
            var request = new HttpRequestMessage(message.Method, message.Url);

            if (message.Headers != null)
                foreach (var header in message.Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

            if (message.Body != null && message.Method == HttpMethod.Get)
                throw new Exception("Can't have a payload on GET Request");

            if (message.Body != null)
                request.Content = new FormUrlEncodedContent(message.Body);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NoneSuccessFullStatusCodeException(response.StatusCode, response.ReasonPhrase);

            var jsonString = await response.Content.ReadAsStringAsync();
            return jsonString;
        }
    }
}