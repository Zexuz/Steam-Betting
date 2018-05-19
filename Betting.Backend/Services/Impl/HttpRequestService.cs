using System.Net.Http;
using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;

namespace Betting.Backend.Services.Impl
{
    public class HttpRequestService : IHttpRequestService
    {
        public async Task<string> MakeRequest(string url)
        {
            var response = await new HttpClient().GetStringAsync(url);
            return response;
        }
    }
}