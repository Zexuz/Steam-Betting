using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IBetRepoService
    {
        Task<List<DatabaseModel.Bet>> FindAsync(DatabaseModel.User user);
        Task<List<DatabaseModel.Bet>> FindAsync(DatabaseModel.Match match);
        Task<List<DatabaseModel.Bet>> FindAsync(int matchId, int gameModeId);
        Task<DatabaseModel.Bet>       FindAsync(DatabaseModel.Match match, DatabaseModel.User user);
        Task<DatabaseModel.Bet>       InsertAsync(DatabaseModel.Bet bet, ITransactionWrapper transactionWrapper = null);
        Task<List<DatabaseModel.Bet>> FindAsync(DatabaseModel.User user, int limit, int? from);
        Task<List<int>>               FindAsync(DatabaseModel.User user, List<int> matchIds, int gameModeId);
        Task<List<DatabaseModel.Bet>> FindAsync(List<LookUpGameModeBet> lookUpGameModeBets);
    }
}