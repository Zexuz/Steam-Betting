using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using RpcCommunication;
using Item = RpcCommunication.Item;

namespace Betting.Backend.Services.Interfaces
{
    public interface ISteamService
    {
        Task<GetPlayerSteamInventoryResponse> GetPlayerSteamInventoryAsync(string steamId, int appId, string contextId);
        Task<GetPlayerInfoResponse>           GetPlayerInfoAsync(string steamId);
        Task<List<MakeOfferResponse>>         MakeWithdrawOfferAsync(string steamId, List<AssetAndDescriptionId> ids);
        Task<List<MakeOfferResponse>>         MakeDepositOfferAsync(string steamId, List<Item> unsafeItemsWithoutMarketHashName);
        Task<SellItemsFromOpskinsBotResponse> SellItemsAsync(SellItemsFromOpskinsBotRequest request);
        Task<WithdrawBtcOpskinsResponse>      WithdrawBtcOpskinsAsync(WithdrawBtcOpskinsRequest request);
        Task<AccountBalanceOpskinsResponse>   AccountBalanceOpskinsAsync(AccountBalanceOpskinsRequest request);
        Task<StartAllBotsResponse>            StartAllBotsAsync(StartAllBotsRequest request);
        Task<StopAllBotsResponse>             StopAllBotsAsync(StopAllBotsRequest request);
        Task<GetOfferLoggResponse>            GetOfferLogg(GetOfferLoggRequest request);
        Task<GetOpskinsLoggResponse>          GetOpskinsLogg(GetOpskinsLoggRequest request);
        Task<GetExceptionLoggResponse>        GetExceptionLog(GetExceptionLoggRequest request);
        Task<GetBotLoggResponse>              GetBotLogg(GetBotLoggRequest request);
        Task<GetBotLoginInfoResponse>         GetBotLoginInfo(GetBotLoginInfoRequest request);
        Task<MakeOfferResponse>               MakeOfferAsync(MakeOfferRequest request);
    }
}