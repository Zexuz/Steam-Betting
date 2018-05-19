using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IItemInOfferTransactionRepoService
    {
        Task<List<DatabaseModel.ItemInOfferTransaction>> FindAsync(DatabaseModel.OfferTransaction offerTransaction);
        Task<List<DatabaseModel.ItemInOfferTransaction>> FindAsync(List<DatabaseModel.OfferTransaction> offerTransactions);
        Task<DatabaseModel.ItemInOfferTransaction>       InsertAsync(DatabaseModel.ItemInOfferTransaction itemInOfferTransaction);

        Task InsertAsync(List<DatabaseModel.ItemInOfferTransaction> itemInOfferTransactions,
                         ITransactionWrapper transactionWrapper = null);

        Task<int> Remove(int offerTransactionId);
        Task<int> GetItemCountInOffer(int offerId);
    }
}