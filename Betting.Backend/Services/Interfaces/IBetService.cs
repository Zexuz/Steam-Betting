using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface IBetService
    {
        Task<List<Item>> GetBettedItemsOnMatch(int matchId, int gameModeId);

        Task PlaceBetOnJackpotMatch
        (
            DatabaseModel.Match match,
            JackpotMatchSetting setting,
            DatabaseModel.GameMode gameMode,
            List<AssetAndDescriptionId> assetAndDescriptionIds,
            string userSteamId
        );

        Task PlaceBetOnCoinFlipMatch
        (
            DatabaseModel.CoinFlip match,
            JackpotMatchSetting setting,
            DatabaseModel.GameMode gameMode,
            List<AssetAndDescriptionId> assetAndDescriptionIds,
            string userSteamId
        );
        
        Task PlaceBetOnCoinFlipMatch
        (
            DatabaseModel.CoinFlip match,
            JackpotMatchSetting setting,
            DatabaseModel.GameMode gameMode,
            int assetAndDescriptionIdsCount,
            List<DatabaseModel.Item> items,
            DatabaseModel.User user,
            List<DatabaseModel.ItemDescription> uniqueItemDescriptions
        );
        
        

        Task<List<DatabaseModel.Bet>> GetBetsFromUser(DatabaseModel.User user);
        Task<List<DatabaseModel.Bet>> GetBetsFromUser(DatabaseModel.User user, int limit, int? from);
        Task<List<int>> GetBetsForUserOnMatches(DatabaseModel.User user, List<int> matchIds, int gameModeId);
    }
}