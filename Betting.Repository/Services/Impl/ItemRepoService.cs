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
    public class ItemRepoService : IItemRepoService
    {
        private readonly IItemQueries        _itemQueries;
        private readonly IDatabaseConnection _databaseConnection;


        public ItemRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IItemQueries itemQueries)
        {
            _itemQueries = itemQueries;
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
        }


        public async Task<DatabaseModel.Item> InsertAsync(DatabaseModel.Item item)
        {
            var query = _itemQueries.InsertReturnsId(item);
            var id = (int) await _databaseConnection.ExecuteScalarAsync(query);
            return new DatabaseModel.Item(item.AssetId, item.DescriptionId, item.LocationId, item.OwnerId, item.ReleaseTime, id);
        }

        public async Task InsertAsync(List<DatabaseModel.Item> items, ITransactionWrapper transactionWrapper = null)
        {
            var query = _itemQueries.InsertRange(items);

            if (transactionWrapper != null)
            {
                await transactionWrapper.ExecuteSqlCommand(query);
                return;
            }

            await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<DatabaseModel.Item> FindAsync(int id)
        {
            var query = _itemQueries.GetFromId(id);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.Item>();
            }
        }

        public async Task<List<DatabaseModel.Item>> FindAsync(List<int> ids)
        {
            if (ids.Count == 0)
                return new List<DatabaseModel.Item>();

            var query = _itemQueries.GetItemsFromId(ids);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Item>();
            }
        }

        public async Task<List<DatabaseModel.Item>> FindAsync(DatabaseModel.User user)
        {
            var query = _itemQueries.GetItemsThatUserOwns(user.Id);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Item>();
            }
        }

        public async Task<DatabaseModel.Item> FindAsync(AssetAndDescriptionId info)
        {
            var query = _itemQueries.GetFromAssetId(info);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.Item>();
            }
        }

        public async Task<List<DatabaseModel.Item>> FindAsync(List<AssetAndDescriptionId> infos)
        {
            if (infos.Count == 0)
                return new List<DatabaseModel.Item>();

            var query = _itemQueries.GetItemsFromAssetId(infos);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Item>();
            }
        }

        public async Task<int> ChangeOwner(DatabaseModel.Item item, DatabaseModel.User user)
        {
            return await ChangeOwner(item.Id, user);
        }

        public async Task<int> ChangeOwner(int itemId, DatabaseModel.User user)
        {
            var query = _itemQueries.ChangeOwner(itemId, user.Id);
            return await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<int> ChangeOwner(List<int> itemIds, DatabaseModel.User user)
        {
            if (itemIds.Count == 0) return 0;
            var query = _itemQueries.ChangeOwner(itemIds, user.Id);
            return await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<int> DeleteAsync(List<AssetAndDescriptionId> items)
        {
            var query = _itemQueries.DeleteRange(items);
            return await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<int> DeleteAsync(List<DatabaseModel.Item> items)
        {
            return await DeleteAsync(items.Select(item => new AssetAndDescriptionId
            {
                AssetId = item.AssetId,
                DescriptionId = item.DescriptionId
            }).ToList());
        }

        public async Task<int> DeleteAsync(AssetAndDescriptionId item)
        {
            var query = _itemQueries.DeleteSingle(item);
            return await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<int> DeleteAsync(List<int> ids)
        {
            if (ids.Count == 0) return 0;
            var query = _itemQueries.DeleteRange(ids);
            return await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<List<DatabaseModel.Item>> GetAll()
        {
            var query = _itemQueries.GetAll();
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Item>();
            }
        }

        public async Task<int> ChangeOwner(List<AssetAndDescriptionId> items, DatabaseModel.User user)
        {
            if (items.Count == 0) return 0;
            var query = _itemQueries.ChangeOwner(items, user.Id);
            return await _databaseConnection.ExecuteNonQueryAsync(query);
        }
    }
}