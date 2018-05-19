using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;
using Dapper;
using Database = Betting.Repository.Factories.Database;

namespace Betting.Repository.Services.Impl
{
    public class RakeItemRepoService : IRakeItemRepoService
    {
        private readonly IRakeItemQueries    _rakeItemQueries;
        private readonly IDatabaseConnection _databaseConnection;


        public RakeItemRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IRakeItemQueries rakeItemQueries)
        {
            _rakeItemQueries = rakeItemQueries;
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
        }


        public async Task<DatabaseModel.RakeItem> InsertAsync(DatabaseModel.RakeItem rakeItem)
        {
            var query = _rakeItemQueries.InsertReturnsId(rakeItem);
            var id = (int) await _databaseConnection.ExecuteScalarAsync(query);
            return new DatabaseModel.RakeItem
            (
                rakeItem.AssetId,
                rakeItem.DescriptionId,
                rakeItem.LocationId,
                rakeItem.Received,
                rakeItem.MatchId,
                rakeItem.GameModeId,
                rakeItem.IsSold,
                id
            );
        }

        public async Task InsertAsync(List<DatabaseModel.RakeItem> rakeItems, ITransactionWrapper transactionWrapper = null)
        {
            var query = _rakeItemQueries.InsertRange(rakeItems);

            if (transactionWrapper != null)
            {
                await transactionWrapper.ExecuteSqlCommand(query);
                return;
            }

            await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<DatabaseModel.RakeItem> FindAsync(int id)
        {
            var query = _rakeItemQueries.GetFromId(id);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.RakeItem>();
            }
        }

        public async Task<List<DatabaseModel.RakeItem>> FindAsync(List<int> ids)
        {
            if (ids.Count == 0)
                return new List<DatabaseModel.RakeItem>();

            var query = _rakeItemQueries.GetItemsFromId(ids);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.RakeItem>();
            }
        }

        public async Task<List<DatabaseModel.RakeItem>> FindAsync(DatabaseModel.Match match)
        {
            if (match == null)
                return new List<DatabaseModel.RakeItem>();

            var query = _rakeItemQueries.GetFromMatchId(match.Id);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.RakeItem>();
            }
        }

        public async Task<DatabaseModel.RakeItem> FindAsync(AssetAndDescriptionId info)
        {
            var query = _rakeItemQueries.GetFromAssetId(info);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.RakeItem>();
            }
        }

        public async Task<List<DatabaseModel.RakeItem>> FindAsync(List<AssetAndDescriptionId> infos)
        {
            if (infos.Count == 0)
                return new List<DatabaseModel.RakeItem>();

            var query = _rakeItemQueries.GetItemsFromAssetId(infos);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.RakeItem>();
            }
        }

        public async Task<List<DatabaseModel.RakeItem>> FindFromGameModeIdAsync(List<int> ids)
        {
            using (var cn = _databaseConnection.GetNewOpenConnection())
            {
                var res = await cn.QueryAsync<DatabaseModel.RakeItem>("SELECT * FROM RakeItem WHERE GameModeId IN @ids", new
                {
                    Ids = ids
                });

                return res.ToList();
            }
        }

//        public async Task<int> DeleteAsync(List<AssetAndDescriptionId> rakeItems)
//        {
//            var query = _rakeItemQueries.DeleteRange(rakeItems);
//            return await _databaseConnection.ExecuteNonQueryAsync(query);
//        }
//
//        public async Task<int> DeleteAsync(List<DatabaseModel.RakeItem> rakeItems)
//        {
//            return await DeleteAsync(rakeItems.Select(rakeItem => new AssetAndDescriptionId
//            {
//                AssetId = rakeItem.AssetId,
//                DescriptionId = rakeItem.DescriptionId
//            }).ToList());
//        }
//
//        public async Task<int> DeleteAsync(AssetAndDescriptionId rakeItem)
//        {
//            var query = _rakeItemQueries.DeleteSingle(rakeItem);
//            return await _databaseConnection.ExecuteNonQueryAsync(query);
//        }

        public async Task<List<DatabaseModel.RakeItem>> GetAll()
        {
            var query = _rakeItemQueries.GetAll();
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.RakeItem>();
            }
        }

        public async Task<List<DatabaseModel.RakeItem>> GetAllWithSoldStatus(bool status)
        {
            var query = _rakeItemQueries.GetAllWithSoldStatus(status);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.RakeItem>();
            }
        }

        public async Task SetAsSold(List<string> assetIds)
        {
            var query = _rakeItemQueries.SetAsSold(assetIds);
            await _databaseConnection.ExecuteNonQueryAsync(query);
        }
    }
}