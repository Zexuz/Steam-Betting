using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface ISettingsService
    {
        Task<DatabaseModel.Settings> GetCurrentSettings();
        Task                         SetOrUpdateSettings(DatabaseModel.Settings settings);
        DatabaseModel.Settings       CreateSettingsObject();
        DatabaseModel.Settings       GetEmptySettingsObject();
    }
}