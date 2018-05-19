using System;
using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IOfferTransationQueries
    {
        SqlQuery InsertRange(List<DatabaseModel.OfferTransaction> offerTransactions);
        SqlQuery InsertReturnsId(DatabaseModel.OfferTransaction offerTransactions);
        SqlQuery Insert(DatabaseModel.OfferTransaction itemDescription);
        SqlQuery GetAll();
        SqlQuery GetFromIds(List<int> ids);
        SqlQuery GetFromId(int id);
        SqlQuery GetFromSteamOfferid(string id);
        SqlQuery GetFromUserId(int userId);
        SqlQuery GetFromUserId(int userId, int limit, int? from);
        SqlQuery GetActiveFromUserId(int userId);
        SqlQuery UpdateSteamOfferId(int offerTransactionId, string steamOfferId);
        SqlQuery Remove(int offerTransactionId);
        SqlQuery AddAcceptedTimeOnOfferWithOfferId(DateTime time, int id);
    }
}