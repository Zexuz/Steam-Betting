using System.Threading.Tasks;

namespace Betting.Backend.Services.Interfaces
{
    public interface IHttpRequestService
    {
        Task<string> MakeRequest(string url);
    }
}