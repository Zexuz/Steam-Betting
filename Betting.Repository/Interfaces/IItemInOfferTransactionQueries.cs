using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IItemInOfferTransactionQueries
    {
        SqlQuery InsertRange(List<DatabaseModel.ItemInOfferTransaction> itemInOfferTransactions);
        SqlQuery InsertReturnsId(DatabaseModel.ItemInOfferTransaction itemInOfferTransaction);
        SqlQuery Insert(DatabaseModel.ItemInOfferTransaction itemInOfferTransaction);
        SqlQuery GetAll();
        SqlQuery GetFromIds(List<int> ids);
        SqlQuery GetFromTransactionsIds(List<int> ids);
        SqlQuery GetFromId(int id);
        SqlQuery GetFromTransactionId(int id);
        SqlQuery Remove(int offerTransactionId);
        SqlQuery GetItemCountFromOfferId(int offerId);
    }
}