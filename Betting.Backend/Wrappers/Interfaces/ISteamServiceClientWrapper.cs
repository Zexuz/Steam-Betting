using System.Threading.Tasks;
using RpcCommunication;

namespace Betting.Backend.Wrappers.Interfaces
{
    public interface ISteamServiceClientWrapper:IGrpcBase
    {
        Task<GetPlayerSteamInventoryResponse> GetPlayerSteamInventoryAsync(GetPlayerSteamInventoryRequest request);
        Task<MakeOfferResponse>               MakeOfferAsync(MakeOfferRequest request);
        Task<GetPlayerInfoResponse>           GetPlayerInfoAsync(GetPlayerInfoRequest request);
        Task<SellItemsFromOpskinsBotResponse> SellItemsAsync(SellItemsFromOpskinsBotRequest request);
        Task<WithdrawBtcOpskinsResponse>      WithdrawBtcOpskinsAsync(WithdrawBtcOpskinsRequest request);
        Task<AccountBalanceOpskinsResponse>   AccountBalanceOpskinsAsync(AccountBalanceOpskinsRequest request);
        Task<StartAllBotsResponse>            StartAllBotsAsync(StartAllBotsRequest request);
        Task<StopAllBotsResponse>             StopAllBotsAsync(StopAllBotsRequest request);
        Task<GetOfferLoggResponse>            GetOfferLoggAsync(GetOfferLoggRequest request);
        Task<GetOpskinsLoggResponse>          GetOpskinsLoggAsync(GetOpskinsLoggRequest request);
        Task<GetExceptionLoggResponse>        GetExceptionLoggAsync(GetExceptionLoggRequest request);
        Task<GetBotLoggResponse>              GetBotLoggAsync(GetBotLoggRequest request);
        Task<GetBotLoginInfoResponse>         GetBotLoginInfoAsync(GetBotLoginInfoRequest request);
    }
}