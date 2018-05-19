using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IItemBettedRepoService
    {
        Task<DatabaseModel.ItemBetted>       InsertAsync(DatabaseModel.ItemBetted itemBetted);
        Task                                 InsertAsync(List<DatabaseModel.ItemBetted> itemBet, ITransactionWrapper transactionWrapper = null);
        Task<List<DatabaseModel.ItemBetted>> FindAsync(DatabaseModel.Bet bet);
        Task<List<DatabaseModel.ItemBetted>> FindAsync(List<DatabaseModel.Bet> bets);
        Task<List<DatabaseModel.ItemBetted>> FindAsync(List<int> betIds);
    }
}