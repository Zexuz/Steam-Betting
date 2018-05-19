using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IBetQueries
    {
        SqlQuery InsertRange(List<DatabaseModel.Bet> bets);
        SqlQuery InsertReturnsId(DatabaseModel.Bet bet);
        SqlQuery GetAll();
        SqlQuery GetFromId(int id);
        SqlQuery GetBet(int matchId, int userId, int gameModeId);
        SqlQuery GetAllBetsForMatch(int matchId, int gameModeId);
        SqlQuery GetAllBetsFromUser(int userId);
        SqlQuery Insert(DatabaseModel.Bet bet);
        SqlQuery GetBetsFromUser(int userId, int limit, int? from);
        SqlQuery GetBetIdsFromUserAndMatch(DatabaseModel.User user, List<int> matchIds,int gameModeId);
    }
}