using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IMatchRepoService
    {
        Task<DatabaseModel.Match>       FindAsync(int roundId);
        Task<DatabaseModel.Match>       GetCurrentMatch();
        Task<DatabaseModel.Match>       InsertAsync(DatabaseModel.Match match);
        Task                            StartTimerForMatch(int roundId, DateTime time);
        Task                            AddWinnerToMatch(DatabaseModel.User winner, int matchRoundId);
        Task                            CloseMatch(int roundId);
        Task<List<DatabaseModel.Match>> FindRangeAsync(int id, int i);
        Task<List<DatabaseModel.Match>> GetMatchesUserWon(DatabaseModel.User user, int nrOfItems, int? startFrom);
        Task<List<int>>                 GetMatchIdsUserWon(DatabaseModel.User user);
        Task<List<DatabaseModel.Match>> FindAsync(List<int> roundIds, bool includeOpenMatches = false);
        Task<List<DatabaseModel.Match>> FindByMatchIdsAsync(List<int> matchIds, bool includeOpenMatches = false);
        Task<List<DatabaseModel.Match>> FindAsync(DateTime statsModelStart, DateTime includeOpenMatches);
    }
}