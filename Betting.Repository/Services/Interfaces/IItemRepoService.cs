using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IItemRepoService
    {
        Task<DatabaseModel.Item>       InsertAsync(DatabaseModel.Item item);
        Task                           InsertAsync(List<DatabaseModel.Item> items, ITransactionWrapper transactionWrapper = null);
        Task<DatabaseModel.Item>       FindAsync(int id);
        Task<List<DatabaseModel.Item>> FindAsync(List<int> ids);
        Task<List<DatabaseModel.Item>> FindAsync(DatabaseModel.User user);
        Task<DatabaseModel.Item>       FindAsync(AssetAndDescriptionId info);
        Task<List<DatabaseModel.Item>> FindAsync(List<AssetAndDescriptionId> infos);
        Task<int>                      ChangeOwner(DatabaseModel.Item item, DatabaseModel.User user);
        Task<int>                      ChangeOwner(int itemId, DatabaseModel.User user);
        Task<int>                      ChangeOwner(List<int> itemIds, DatabaseModel.User user);
        Task<int>                      DeleteAsync(List<AssetAndDescriptionId> items);
        Task<int>                      DeleteAsync(List<DatabaseModel.Item> items);
        Task<int>                      DeleteAsync(AssetAndDescriptionId item);
        Task<int>                      DeleteAsync(List<int> rakeResItemIdsToWinner);
        Task<List<DatabaseModel.Item>> GetAll();
        Task<int>                      ChangeOwner(List<AssetAndDescriptionId> items, DatabaseModel.User user);
    }
}