using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IItemBetQueries
    {
        SqlQuery InsertRangeAsync(List<DatabaseModel.ItemBetted> itemBets);
        SqlQuery InsertAsync(DatabaseModel.ItemBetted itemBetted);
        SqlQuery GetItemBetsOnBet(int betId);
        SqlQuery GetItemBetsOnBets(List<int> betIds);
    }
}