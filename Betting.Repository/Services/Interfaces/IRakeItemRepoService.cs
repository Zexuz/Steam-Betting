using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IRakeItemRepoService
    {
        Task<DatabaseModel.RakeItem>       InsertAsync(DatabaseModel.RakeItem rakeItem);
        Task                               InsertAsync(List<DatabaseModel.RakeItem> rakeItems, ITransactionWrapper transactionWrapper = null);
        Task<DatabaseModel.RakeItem>       FindAsync(int id);
        Task<List<DatabaseModel.RakeItem>> FindAsync(List<int> ids);
        Task<DatabaseModel.RakeItem>       FindAsync(AssetAndDescriptionId info);
        Task<List<DatabaseModel.RakeItem>> FindAsync(List<AssetAndDescriptionId> infos);
        Task<List<DatabaseModel.RakeItem>> FindAsync(DatabaseModel.Match match);
        Task<List<DatabaseModel.RakeItem>> FindFromGameModeIdAsync(List<int> ids);
        Task<List<DatabaseModel.RakeItem>> GetAll();
        Task<List<DatabaseModel.RakeItem>> GetAllWithSoldStatus(bool status);
        Task SetAsSold(List<string> assetIds);
    }
}