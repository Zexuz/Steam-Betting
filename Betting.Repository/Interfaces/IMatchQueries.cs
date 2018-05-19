using System;
using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IMatchQueries
    {
        SqlQuery InsertMatches(List<DatabaseModel.Match> matchesToInsert);
        SqlQuery InsertReturnsId(DatabaseModel.Match match);
        SqlQuery GetAllMatches();
        SqlQuery GetMatchFromRoundId(int roundId);
        SqlQuery GetMatchFromId(int id);
        SqlQuery Insert(DatabaseModel.Match match);
        SqlQuery GetCurrentMatch();
        SqlQuery AddTimer(int matchId, DateTime time);
        SqlQuery ChangeMatchStatus(int roundId, int status);
        SqlQuery FindRange(int id, int i);
        SqlQuery AddWinner(int winnerId, int matchRoundId);
        SqlQuery GetMatchesUserWon(int userId, int nrOfItemsToReturn, int? startFrom);
        SqlQuery GetMatcheIdsUserWon(int userId);
        SqlQuery FindRange(List<int> roundIds, bool includeOpenMatches);
        SqlQuery FindRangeByMatchIds(List<int> matchIds, bool includeOpenMatches);
    }
}