using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class BetQueries : IBetQueries
    {
        public SqlQuery InsertRange(List<DatabaseModel.Bet> bets)
        {
            var listOfSqlValues = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < bets.Count; index++)
            {
                listOfSqlValues.Add($"(@userId{index} ,@matchId{index},@created{index}, @gameModeId{index})");
                dict.Add($"@userId{index}", bets[index].UserId);
                dict.Add($"@matchId{index}", bets[index].MatchId);
                dict.Add($"@created{index}", bets[index].Created);
                dict.Add($"@gameModeId{index}", bets[index].GameModeId);
            }

            return new SqlQuery($"INSERT INTO [Bet] (UserId, MatchId, GameModeId) VALUES {string.Join(",", listOfSqlValues)};", dict);
        }

        public SqlQuery InsertReturnsId(DatabaseModel.Bet bet)
        {
            var dict = new Dictionary<string, object>
            {
                {"@userId", bet.UserId},
                {"@matchId", bet.MatchId},
                {"@created", bet.Created},
                {"@gameModeId", bet.GameModeId},
            };

            return new SqlQuery(
                "INSERT INTO [Bet] (UserId, MatchId, Created, GameModeId) OUTPUT INSERTED.Id VALUES (@userId, @matchId, @created,@gameModeId);",
                dict);
        }

        public SqlQuery GetAll()
        {
            return new SqlQuery("SELECT * FROM [Bet]", null);
        }

        public SqlQuery GetFromId(int id)
        {
            return new SqlQuery("SELECT * FROM [Bet] WHERE Id =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery GetBet(int matchId, int userId, int gameModeId)
        {
            return new SqlQuery("SELECT * FROM [Bet] WHERE MatchId =@matchId AND UserId =@userId AND GameModeId = @gameModeId",
                new Dictionary<string, object>
                {
                    {"@matchId", matchId},
                    {"@gameModeId", gameModeId},
                    {"@userId", userId}
                });
        }

        public SqlQuery GetAllBetsForMatch(int matchId, int gameModeId)
        {
            return new SqlQuery("SELECT * FROM [Bet] WHERE MatchId =@matchId AND GameModeId = @gameModeId",
                new Dictionary<string, object>
                {
                    {"@matchId", matchId},
                    {"@gameModeId", gameModeId}
                });
        }

        public SqlQuery GetAllBetsFromUser(int userId)
        {
            return new SqlQuery("SELECT * FROM [Bet] WHERE UserId =@userId", new Dictionary<string, object> {{"@userId", userId}});
        }

        public SqlQuery Insert(DatabaseModel.Bet bet)
        {
            return InsertRange(new List<DatabaseModel.Bet> {bet});
        }

        public SqlQuery GetBetsFromUser(int userId, int limit, int? from)
        {
            if (from.HasValue)
                return new SqlQuery("SELECT TOP(@nr) * FROM [Bet] WHERE UserId =@userId AND MatchId > @from ORDER BY MatchId DESC",
                    new Dictionary<string, object>
                    {
                        {"@userId", userId},
                        {"@nr", limit},
                        {"@from", from.Value}
                    });

            return new SqlQuery("SELECT TOP(@nr) * FROM [Bet] WHERE UserId =@userId ORDER BY MatchId DESC", new Dictionary<string, object>
            {
                {"@userId", userId},
                {"@nr", limit},
            });
        }

        public SqlQuery GetBetIdsFromUserAndMatch(DatabaseModel.User user, List<int> matchIds, int gameModeId)
        {
            var ids = string.Join(",", matchIds);
            return new SqlQuery(
                $"SELECT * FROM [Bet] WHERE UserId = @userId AND GameModeId =@gameModeId AND MatchId IN ({ids}) ORDER BY MatchId DESC",
                new Dictionary<string, object>
                {
                    {"@userId", user.Id},
                    {"@gameModeId", gameModeId},
                });
        }
    }
}