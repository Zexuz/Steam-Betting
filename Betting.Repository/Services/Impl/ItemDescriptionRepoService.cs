using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;
using Dapper;

namespace Betting.Repository.Services.Impl
{
    public class ItemDescriptionRepoService : IItemDescriptionRepoService
    {
        private readonly IItemDescriptionQueries _itemDescriptionQueries;
        private readonly IDatabaseConnection     _databaseConnection;

        public ItemDescriptionRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IItemDescriptionQueries itemDescriptionQueries)
        {
            _itemDescriptionQueries = itemDescriptionQueries;
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
        }

        public async Task<List<DatabaseModel.ItemDescription>> GetAll()
        {
            var query = _itemDescriptionQueries.GetAll();
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.ItemDescription>();
            }
        }

        public async Task<DatabaseModel.ItemDescription> FindAsync(int id)
        {
            var query = _itemDescriptionQueries.GetFromId(id);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.ItemDescription>();
            }
        }

        public async Task<List<DatabaseModel.ItemDescription>> FindAsync(List<int> ids)
        {
            if (ids.Count == 0)
                return new List<DatabaseModel.ItemDescription>();

            var query = _itemDescriptionQueries.GetFromIds(ids);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.ItemDescription>();
            }
        }

        public async Task<DatabaseModel.ItemDescription> FindAsync(string name)
        {
            var query = _itemDescriptionQueries.GetFromName(name);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.ItemDescription>();
            }
        }

        public async Task<List<DatabaseModel.ItemDescription>> FindAsync(List<string> names)
        {
            if (names.Count == 0)
                return new List<DatabaseModel.ItemDescription>();

            var query = _itemDescriptionQueries.GetFromNames(names);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.ItemDescription>();
            }
        }

        public async Task<DatabaseModel.ItemDescription> InsertAsync(DatabaseModel.ItemDescription itemDescription)
        {
            var query = _itemDescriptionQueries.InsertReturnsId(itemDescription);
            var id = (int) await _databaseConnection.ExecuteScalarAsync(query);
            return new DatabaseModel.ItemDescription(
                itemDescription.Name,
                itemDescription.Value,
                itemDescription.AppId,
                itemDescription.ContextId,
                itemDescription.ImageUrl,
                itemDescription.Valid,
                id
            );
        }

        public async Task UpdateImg(string name, string imgUrl)
        {
            var query = _itemDescriptionQueries.UpdateImgUrl(name, imgUrl);
            await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task Update(DatabaseModel.ItemDescription itemDesc)
        {
            var query = _itemDescriptionQueries.Update(itemDesc);
            var res = await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<DatabaseModel.ItemDescription> InsertOrUpdate(DatabaseModel.ItemDescription itemDesc)
        {
            var findRes = await FindAsync(itemDesc.Name);
            if (findRes == null)
            {
                return await InsertAsync(itemDesc);
            }

            await Update(itemDesc);
            return new DatabaseModel.ItemDescription
            (
                itemDesc.Name,
                itemDesc.Value,
                itemDesc.AppId,
                itemDesc.ContextId,
                itemDesc.ImageUrl,
                itemDesc.Valid,
                findRes.Id
            );
        }

        public async Task<Dictionary<int, decimal>> ValueOfItemDescriptions(List<int> ids)
        {
            using (var cn = _databaseConnection.GetNewOpenConnection())
            {
                var res = await cn.ExecuteReaderAsync("SELECT Id, Value FROM ItemDescription WHERE Id IN @ids", new {Ids = ids});
                var dict = new Dictionary<int, decimal>();
                while (res.Read())
                {
                    var id = res.GetInt32(0);
                    var value = res.GetDecimal(1);
                    dict.Add(id, value);
                }

                cn.Close();


                return dict;
            }
        }

        public async Task RemoveItemsWithNoImage()
        {
            using (var cn = _databaseConnection.GetNewOpenConnection())
            {
                await cn.ExecuteAsync("DELETE FROM ItemDescription WHERE ImageUrl = 'noImg'");
                cn.Close();
            }
        }

        public async Task InvalidateItemForAppId(int appId)
        {
            using (var cn = _databaseConnection.GetNewOpenConnection())
            {
                await cn.ExecuteAsync("UPDATE ItemDescription SET Valid = 0 WHERE AppId = @appId",new {AppId = appId});
                cn.Close();
            }
        }
    }
}