using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface IItemTransferService
    {
        Task<bool> TransferItemsAsync(DatabaseModel.User fromUser, string toSteamId, List<AssetAndDescriptionId> items);
    }
}