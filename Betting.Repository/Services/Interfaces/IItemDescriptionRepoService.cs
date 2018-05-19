using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IItemDescriptionRepoService
    {
        Task<List<DatabaseModel.ItemDescription>> GetAll();
        Task<DatabaseModel.ItemDescription>       FindAsync(int id);
        Task<List<DatabaseModel.ItemDescription>> FindAsync(List<int> ids);
        Task<DatabaseModel.ItemDescription>       FindAsync(string name);
        Task<List<DatabaseModel.ItemDescription>> FindAsync(List<string> names);
        Task<DatabaseModel.ItemDescription>       InsertAsync(DatabaseModel.ItemDescription itemDescription);
        Task                                      UpdateImg(string name, string imgUrl);
        Task                                      Update(DatabaseModel.ItemDescription itemDesc);
        Task<DatabaseModel.ItemDescription>       InsertOrUpdate(DatabaseModel.ItemDescription itemDesc);
        Task<Dictionary<int, decimal>>            ValueOfItemDescriptions(List<int> ids);
        Task                                      RemoveItemsWithNoImage();
        Task InvalidateItemForAppId(int appId);
    }
}