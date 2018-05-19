using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Services.Impl;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface IJackpotDraftService
    {
        WinningBet GetWinningBet(double percantage, List<DatabaseModel.Bet> bets, List<DatabaseModel.ItemBetted> itemBets);

        Task ChangeOwnerOfItems(
            List<DatabaseModel.Bet> bets,
            List<DatabaseModel.ItemBetted> itemBets,
            DatabaseModel.User winningUser,
            int matchId,
            decimal rake,
            int gameModeId
        );
    }
}