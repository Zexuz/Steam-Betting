using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Services.Impl
{
    public class OfferTransactionRepoService : IOfferTranascrionRepoService
    {
        private readonly IDatabaseConnection     _databaseConnection;
        private readonly IOfferTransationQueries _offerTransationQueries;

        public OfferTransactionRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IOfferTransationQueries offerTransationQueries)
        {
            _databaseConnection     = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
            _offerTransationQueries = offerTransationQueries;
        }

        public async Task<DatabaseModel.OfferTransaction> InsertAsync(DatabaseModel.OfferTransaction offerTransaction,
                                                                      ITransactionWrapper transactionWrapper = null)
        {
            var query = _offerTransationQueries.InsertReturnsId(offerTransaction);

            int id;
            if (transactionWrapper != null)
                id = await transactionWrapper.ExecuteSqlCommand<int>(query);
            else
                id = (int) await _databaseConnection.ExecuteScalarAsync(query);

            return new DatabaseModel.OfferTransaction(
                offerTransaction.UserId,
                offerTransaction.BotId,
                offerTransaction.TotalValue,
                offerTransaction.IsDeposit,
                offerTransaction.SteamOfferId,
                offerTransaction.Accepted,
                id
            );
        }

        public async Task<List<DatabaseModel.OfferTransaction>> FindAsync(DatabaseModel.User user)
        {
            var query = _offerTransationQueries.GetFromUserId(user.Id);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var offerTransaction = await sqlRes.GetListAsync<DatabaseModel.OfferTransaction>();
                return offerTransaction;
            }
        }

        public async Task<List<DatabaseModel.OfferTransaction>> FindAsync(DatabaseModel.User user, int limit, int? from)
        {
            var query = _offerTransationQueries.GetFromUserId(user.Id, limit, from);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var offerTransaction = await sqlRes.GetListAsync<DatabaseModel.OfferTransaction>();
                return offerTransaction;
            }
        }

        public async Task<DatabaseModel.OfferTransaction> FindAsync(string steamOfferId)
        {
            var query = _offerTransationQueries.GetFromSteamOfferid(steamOfferId);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var offerTransaction = await sqlRes.GetSingleAsync<DatabaseModel.OfferTransaction>();
                return offerTransaction;
            }
        }

        public async Task<List<DatabaseModel.OfferTransaction>> FindActiveAsync(DatabaseModel.User user)
        {
            var query = _offerTransationQueries.GetActiveFromUserId(user.Id);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var offerTransaction = await sqlRes.GetListAsync<DatabaseModel.OfferTransaction>();
                return offerTransaction;
            }
        }

        public async Task<DatabaseModel.OfferTransaction> FindAsync(int id)
        {
            var query = _offerTransationQueries.GetFromId(id);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var offerTransaction = await sqlRes.GetSingleAsync<DatabaseModel.OfferTransaction>();
                return offerTransaction;
            }
        }

        public async Task AddSteamIdToOffer(int offerTransactionId, string steamOfferId)
        {
            var query = _offerTransationQueries.UpdateSteamOfferId(offerTransactionId, steamOfferId);
            var res   = await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task Remove(int offerTransactionId)
        {
            var query = _offerTransationQueries.Remove(offerTransactionId);
            var res   = await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task AddAcceptedTimesptampToOffer(DateTime time, int id)
        {
            var query = _offerTransationQueries.AddAcceptedTimeOnOfferWithOfferId(time, id);
            var res   = await _databaseConnection.ExecuteNonQueryAsync(query);
        }
    }
}