using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface IBotService
    {
        Task<Dictionary<DatabaseModel.Bot, List<DatabaseModel.Item>>> GetBotsWithWithdrawItems(List<AssetAndDescriptionId> ids);

        Task<Stack<DatabaseModel.Bot>> GetAvalibleBotsForDeposit(
            DatabaseModel.User user, List<DatabaseModel.Item> usersItems);
    }
}