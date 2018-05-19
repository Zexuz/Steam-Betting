using System.Threading.Tasks;
using RpcCommunicationHistory;

namespace Betting.Backend.Wrappers.Interfaces
{
    public interface IHistoryServiceClientWrapper:IGrpcBase
    {
        Task<MatchResponse> GetGlobalMatchHistoryAsync(GetGlobalHistoryRequest request);
        Task<MatchResponse> GetPersonalMatchHistoryAsync(GetPersonalHistoryRequest request);
    }
}