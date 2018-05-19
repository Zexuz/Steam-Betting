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
    public class ItemBettedRepoService : IItemBettedRepoService
    {
        private readonly IDatabaseConnection _databaseConnection;
        private readonly IItemBetQueries     _itemBetQueries;

        public ItemBettedRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IItemBetQueries itemBetQueries)
        {
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
            _itemBetQueries     = itemBetQueries;
        }

        public async Task<DatabaseModel.ItemBetted> InsertAsync(DatabaseModel.ItemBetted itemBetted)
        {
            var query = _itemBetQueries.InsertAsync(itemBetted);
            await _databaseConnection.ExecuteNonQueryAsync(query);
            return itemBetted;
        }

        public async Task<List<DatabaseModel.ItemBetted>> FindAsync(DatabaseModel.Bet bet)
        {
            var query = _itemBetQueries.GetItemBetsOnBet(bet.Id);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.ItemBetted>();
            }
        }

        public async Task<List<DatabaseModel.ItemBetted>> FindAsync(List<DatabaseModel.Bet> bets)
        {
            if (bets.Count == 0)
                return new List<DatabaseModel.ItemBetted>();

            var betIds = bets.Select(bet => bet.Id).ToList();
            return await FindAsync(betIds);
        }
        
        public async Task<List<DatabaseModel.ItemBetted>> FindAsync(List<int> betIds)
        {
            if (betIds.Count == 0)
                return new List<DatabaseModel.ItemBetted>();

            var query  = _itemBetQueries.GetItemBetsOnBets(betIds);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.ItemBetted>();
            }
        }


        public async Task InsertAsync(List<DatabaseModel.ItemBetted> itemBet, ITransactionWrapper transactionWrapper = null)
        {
            var query = _itemBetQueries.InsertRangeAsync(itemBet);

            if (transactionWrapper != null)
            {
                await transactionWrapper.ExecuteSqlCommand(query);
                return;
            }

            await _databaseConnection.ExecuteNonQueryAsync(query);
        }
    }
}