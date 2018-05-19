using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Services.Impl
{
    public class ItemInOfferTransactionRepoService : IItemInOfferTransactionRepoService
    {
        private readonly IDatabaseConnection            _databaseConnection;
        private readonly IItemInOfferTransactionQueries _itemInOfferTransactionQueries;

        public ItemInOfferTransactionRepoService(IDatabaseConnectionFactory databaseConnectionFactory,
                                                 IItemInOfferTransactionQueries itemInOfferTransactionQueries)
        {
            _databaseConnection            = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
            _itemInOfferTransactionQueries = itemInOfferTransactionQueries;
        }

        public async Task<List<DatabaseModel.ItemInOfferTransaction>> FindAsync(List<DatabaseModel.OfferTransaction> offerTransactions)
        {
            if (offerTransactions.Count == 0) return new List<DatabaseModel.ItemInOfferTransaction>();

            var ids   = offerTransactions.Select(offerTransaction => offerTransaction.Id).ToList();
            var query = _itemInOfferTransactionQueries.GetFromTransactionsIds(ids);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var items = await sqlRes.GetListAsync<DatabaseModel.ItemInOfferTransaction>();
                return items;
            }
        }

        public async Task<DatabaseModel.ItemInOfferTransaction> InsertAsync(DatabaseModel.ItemInOfferTransaction itemInOfferTransaction)
        {
            var query = _itemInOfferTransactionQueries.InsertReturnsId(itemInOfferTransaction);
            var id    = (int) await _databaseConnection.ExecuteScalarAsync(query);
            return new DatabaseModel.ItemInOfferTransaction(
                itemInOfferTransaction.OfferTransactionId,
                itemInOfferTransaction.ItemDescriptionId,
                itemInOfferTransaction.AssetId,
                itemInOfferTransaction.Value,
                id
            );
        }

        public async Task InsertAsync(List<DatabaseModel.ItemInOfferTransaction> itemInOfferTransactions, ITransactionWrapper transactionWrapper = null)
        {
            var query = _itemInOfferTransactionQueries.InsertRange(itemInOfferTransactions);

            if (transactionWrapper != null)
            {
                await transactionWrapper.ExecuteSqlCommand(query);
                return;
            }

            await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<int> Remove(int offerTransactionId)
        {
            var query = _itemInOfferTransactionQueries.Remove(offerTransactionId);
            return await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<int> GetItemCountInOffer(int offerId)
        {
            var query = _itemInOfferTransactionQueries.GetItemCountFromOfferId(offerId);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlRes.GetSingleAsync<int>();
            }
        }

        public async Task<List<DatabaseModel.ItemInOfferTransaction>> FindAsync(DatabaseModel.OfferTransaction offerTransaction)
        {
            var query = _itemInOfferTransactionQueries.GetFromTransactionId(offerTransaction.Id);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var items = await sqlRes.GetListAsync<DatabaseModel.ItemInOfferTransaction>();
                return items;
            }
        }
    }
}