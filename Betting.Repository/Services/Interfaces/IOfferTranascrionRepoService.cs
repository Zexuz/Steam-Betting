using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IOfferTranascrionRepoService
    {
        Task<DatabaseModel.OfferTransaction>       InsertAsync(DatabaseModel.OfferTransaction offerTransaction, ITransactionWrapper transactionWrapper = null);
        Task<List<DatabaseModel.OfferTransaction>> FindAsync(DatabaseModel.User user);
        Task<List<DatabaseModel.OfferTransaction>> FindAsync(DatabaseModel.User user, int limit, int? from);
        Task<DatabaseModel.OfferTransaction>       FindAsync(string steamOfferId);
        Task<List<DatabaseModel.OfferTransaction>> FindActiveAsync(DatabaseModel.User user);
        Task<DatabaseModel.OfferTransaction>       FindAsync(int id);
        Task                                       AddSteamIdToOffer(int offerTransactionId, string steamOfferId);
        Task                                       Remove(int offerTransactionId);
        Task                                       AddAcceptedTimesptampToOffer(DateTime time, int id);
    }
}