using System.Threading.Tasks;
using Betting.Backend.Interfaces;
using Betting.Backend.Wrappers.Interfaces;
using RpcCommunicationHistory;

namespace Betting.Backend.Wrappers
{
    public class HistoryServiceClientWrapper : GrpcClientWrapperBase<HistoryServiceClientWrapper>, IHistoryServiceClientWrapper
    {
        private readonly Bettingv1HisotryService.Bettingv1HisotryServiceClient _histotyClient;

        public HistoryServiceClientWrapper
        (
            Bettingv1HisotryService.Bettingv1HisotryServiceClient histotyClient,
            ILogServiceFactory logServiceFactory
        )
            : base(logServiceFactory.CreateLogger<HistoryServiceClientWrapper>())
        {
            _histotyClient = histotyClient;
        }

        public async Task<MatchResponse> GetGlobalMatchHistoryAsync(GetGlobalHistoryRequest request)
        {
            return await SendGrpcAction(async () => await _histotyClient.GetGlobalHistoryAsync(request, DefaultSettings.GetDefaultSettings(5)));
        }

        public async Task<MatchResponse> GetPersonalMatchHistoryAsync(GetPersonalHistoryRequest request)
        {
            return await SendGrpcAction(async () => await _histotyClient.GetPersonalHistoryAsync(request, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task PingAsync()
        {
            return SendGrpcAction(async () => await _histotyClient.PingAsync(new PingRequest(), DefaultSettings.GetDefaultSettings(2)));
        }
    }
}