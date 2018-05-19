using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface ISettingRepoService
    {
        Task<DatabaseModel.Settings> GetSettingsAsync();
        Task                         SetSettingsAsync(DatabaseModel.Settings settings);
    }
}