using System.Threading.Tasks;
using Betting.Backend.Cache;
using Betting.Backend.Interfaces;
using Betting.Backend.Wrappers.Interfaces;
 
using RpcCommunication;

namespace Betting.Backend.Wrappers
{
    public class SteamServiceClientWrapper : GrpcClientWrapperBase<SteamServiceClientWrapper>, ISteamServiceClientWrapper
    {
        private readonly SteamService.SteamServiceClient _steamServiceClient;
        private readonly ISteamInventoryCacheManager     _cacheManager;

        public SteamServiceClientWrapper
        (
            SteamService.SteamServiceClient steamServiceClient,
            ISteamInventoryCacheManager cacheManager,
            ILogServiceFactory logServiceFactory
        )
            : base(logServiceFactory.CreateLogger<SteamServiceClientWrapper>())
        {
            _steamServiceClient = steamServiceClient;
            _cacheManager = cacheManager;
        }

        public async Task<GetPlayerSteamInventoryResponse> GetPlayerSteamInventoryAsync(GetPlayerSteamInventoryRequest request)
        {
            var steamId = request.SteamId;
            if (_cacheManager.HasCache($"{steamId}:{request.InventoryToFetch.AppId}"))
                return _cacheManager.LookupCache($"{steamId}:{request.InventoryToFetch.AppId}");

            var inventoryResponse = await SendGrpcAction(async () =>
                await _steamServiceClient.GetPlayerSteamInventoryAsync(request, DefaultSettings.GetDefaultSettings(30)));

            _cacheManager.AddToCache($"{steamId}:{request.InventoryToFetch.AppId}", inventoryResponse);
            return inventoryResponse;
        }

        public async Task<MakeOfferResponse> MakeOfferAsync(MakeOfferRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.MakeOfferAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<GetPlayerInfoResponse> GetPlayerInfoAsync(GetPlayerInfoRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.GetPlayerInfoAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<SellItemsFromOpskinsBotResponse> SellItemsAsync(SellItemsFromOpskinsBotRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.SellItemsAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<WithdrawBtcOpskinsResponse> WithdrawBtcOpskinsAsync(WithdrawBtcOpskinsRequest request)
        {
            return await SendGrpcAction(
                async () => await _steamServiceClient.WithdrawBtcOpskinsAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<AccountBalanceOpskinsResponse> AccountBalanceOpskinsAsync(AccountBalanceOpskinsRequest request)
        {
            return await SendGrpcAction(async () =>
                await _steamServiceClient.AccountBalanceOpskinsAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<StartAllBotsResponse> StartAllBotsAsync(StartAllBotsRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.StartAllBotsAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<StopAllBotsResponse> StopAllBotsAsync(StopAllBotsRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.StopAllBotsAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<GetOfferLoggResponse> GetOfferLoggAsync(GetOfferLoggRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.GetOfferLoggAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<GetOpskinsLoggResponse> GetOpskinsLoggAsync(GetOpskinsLoggRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.GetOpskinsLoggAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<GetExceptionLoggResponse> GetExceptionLoggAsync(GetExceptionLoggRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.GetExceptionLoggAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<GetBotLoggResponse> GetBotLoggAsync(GetBotLoggRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.GetBotLoggAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public async Task<GetBotLoginInfoResponse> GetBotLoginInfoAsync(GetBotLoginInfoRequest request)
        {
            return await SendGrpcAction(async () => await _steamServiceClient.GetBotLoginInfoAsync(request, DefaultSettings.GetDefaultSettings(30)));
        }

        public Task PingAsync()
        {
            return SendGrpcAction(async () => await _steamServiceClient.PingAsync(new EmptyMessage(), DefaultSettings.GetDefaultSettings(2)));
        }
    }
}