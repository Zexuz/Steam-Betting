using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models;
using Betting.Models.Models;
using Shared.Shared;

namespace Betting.Repository.Services
{
    public interface IMongoJackpotRepoService
    {
        Task                      SetWinnerTicketForMatch(DatabaseModel.CoinFlip match, int ticket);
        Task                      SetWinnerForMatch(DatabaseModel.CoinFlip match, UserWithQuote user);
        Task                      UpdateTimmerStartedForMatch(DatabaseModel.CoinFlip match, DateTime timer);
        Task                      UpdateStatusForMatch(DatabaseModel.CoinFlip match, MatchStatus status);
        Task                      AddBetToMatch(DatabaseModel.CoinFlip match, CoinFlipBet bet);
        Task<List<CoinFlipMatch>> GetAllOpenOrDraftinMatchesFromMongoDb();
        Task<CoinFlipMatchHistory> FindMatchFromRoundId(int lookUpId);
        Task                      InsertAsync(MongoDbModels.JackpotMatch coinFlipMatchModel);

        Task<Result<CoinFlipMatchHistory>> GetHistory(int start, int count);
        Task<Result<CoinFlipMatchHistory>> GetPersonalHistory(int start, int count, string steamId);
    }
}