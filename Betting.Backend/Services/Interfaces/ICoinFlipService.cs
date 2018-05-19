using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using Shared.Shared;

namespace Betting.Backend.Services.Interfaces
{
    public interface ICoinFlipService
    {
        Task<CoinFlipMatch> CreateMatch
        (string creatorSteamId,
         bool creatorIsHead,
         List<AssetAndDescriptionId> assetAndDescriptionIds,
         CreateCoinFlipSettingModel setting);

        Task<List<DatabaseModel.CoinFlip>> FindAllOpenMatchs();
        Task<List<DatabaseModel.CoinFlip>> FindAllOpenMatchs(DatabaseModel.User user);
        Task<List<CoinFlipMatch>>          GetAllOpenOrDraftinMatchesFromMongoDb();
        Task<Result<CoinFlipMatchHistory>> GetGlobalHistory(int start, int count);
        Task<Result<CoinFlipMatchHistory>> GetPersonalHistory(int start, int count, string steamId);

        Task<string>                CreatePreHash(string steamId);
        Task<MongoDbModels.PreHash> FindPreHash(string hash, string steamId);
        Task<CoinFlipMatchHistory> GetMatchAsync(int lookUpId);
    }
}