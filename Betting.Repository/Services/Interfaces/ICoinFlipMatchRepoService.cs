using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface ICoinFlipMatchRepoService
    {
        Task<DatabaseModel.CoinFlip> InsertAsync(DatabaseModel.CoinFlip coinFlip, ITransactionWrapper transactionTransaction = null);
        Task                         UpdateAsync(DatabaseModel.CoinFlip coinflip);
        Task<DatabaseModel.CoinFlip> FindAsync(int id);
        Task<List<DatabaseModel.CoinFlip>> FindAllOpenMatchesAsync();
        Task<List<DatabaseModel.CoinFlip>> FindAllOpenMatchesAsync(DatabaseModel.User user);
        Task<List<DatabaseModel.CoinFlip>> FindAllNotClosedMatches();
        Task RemoveAsync(DatabaseModel.CoinFlip coinFlip);
    }
}