using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Backend.Managers.Interface
{
    public interface ICoinFlipManager
    {
        Task<List<CoinFlipMatch>> GetAllOpenOrDratingCoinFlipMatches();
        Task PlaceBet(List<AssetAndDescriptionId> assetAndDescriptionIds, int lookUpId, string steamId);
        void                      Start();
        Task                      Stop();
    }
}