using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using RpcCommunication;
using Item = RpcCommunication.Item;

namespace Betting.Backend.Services.Interfaces
{
    public interface IOfferService
    {
        Task DepositSteamOfferAsync(OfferStatusRequest request);
        Task WithdrawalSteamOffer(OfferStatusRequest request);

        Task<DatabaseModel.OfferTransaction> PrepareWithdrawlSteamOffer(List<Item> itemsInOfferRequest, DatabaseModel.Bot bot,
                                                                        DatabaseModel.User owner);

        Task RemoveCanceledWithdrawalSteamOffer(OfferStatusRequest request);
    }
}