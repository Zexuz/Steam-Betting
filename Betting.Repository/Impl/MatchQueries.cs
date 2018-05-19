using System;
using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class MatchQueries : IMatchQueries
    {
        public SqlQuery InsertMatches(List<DatabaseModel.Match> matchesToInsert)
        {
            var listOfSqlValues = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < matchesToInsert.Count; index++)
            {
                listOfSqlValues.Add(
                    $"(@roundId{index} ,@salt{index}, @hash{index}, @percentage{index},@created{index},@timerStarted{index},@winnerId{index}, @status{index})");
                dict.Add($"@roundId{index}", matchesToInsert[index].RoundId);
                dict.Add($"@salt{index}", matchesToInsert[index].Salt);
                dict.Add($"@hash{index}", matchesToInsert[index].Hash);
                dict.Add($"@percentage{index}", matchesToInsert[index].Percentage);
                dict.Add($"@created{index}", matchesToInsert[index].Created);
                dict.Add($"@timerStarted{index}", matchesToInsert[index].TimerStarted);
                dict.Add($"@winnerId{index}", matchesToInsert[index].WinnerId);
                dict.Add($"@status{index}", matchesToInsert[index].Status);
            }

            return new SqlQuery(
                $"INSERT INTO [Match] (RoundId, Salt, Hash, Percentage,Created,TimerStarted,WinnerId,Status) VALUES {string.Join(",", listOfSqlValues)};",
                dict);
        }

        public SqlQuery InsertReturnsId(DatabaseModel.Match match)
        {
            var dict = new Dictionary<string, object>
            {
                {"@rId", match.RoundId},
                {"@salt", match.Salt},
                {"@h", match.Hash},
                {"@perc", match.Percentage},
                {"@crated", match.Created},
                {"@timerStarted", match.TimerStarted},
                {"@winnerId", match.WinnerId},
                {"@s", match.Status},
            };

            return new SqlQuery("INSERT INTO [Match] (RoundId, Salt, Hash, Percentage,Created,TimerStarted, WinnerId, Status)" +
                                " OUTPUT INSERTED.Id VALUES (@rId,@salt, @h, @perc,@crated,@timerStarted,@winnerId, @s);", dict);
        }

        public SqlQuery GetAllMatches()
        {
            return new SqlQuery("SELECT * FROM [Match]", null);
        }

        public SqlQuery GetMatchFromRoundId(int roundId)
        {
            return new SqlQuery("SELECT * FROM [Match] WHERE RoundId =@roundId", new Dictionary<string, object> {{"@roundId", roundId}});
        }

        public SqlQuery GetMatchFromId(int id)
        {
            return new SqlQuery("SELECT * FROM [Match] WHERE Id =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery Insert(DatabaseModel.Match match)
        {
            var list = new List<DatabaseModel.Match> {match};
            return InsertMatches(list);
        }

        public SqlQuery GetCurrentMatch()
        {
            return new SqlQuery("SELECT TOP 1 * FROM [Match] ORDER BY RoundId DESC;", null);
        }

        public SqlQuery AddTimer(int roundId, DateTime time)
        {
            return new SqlQuery("UPDATE [Match] SET TimerStarted = @timer WHERE roundId = @roundId;", new Dictionary<string, object>
            {
                {"@roundId", roundId},
                {"@timer", time}
            });
        }

        public SqlQuery ChangeMatchStatus(int roundId, int status)
        {
            return new SqlQuery("UPDATE [Match] SET Status = @status WHERE roundId = @roundId;", new Dictionary<string, object>
            {
                {"@roundId", roundId},
                {"@status", status}
            });
        }

        public SqlQuery FindRange(int id, int i)
        {
            return new SqlQuery(
                "SELECT TOP (@nr) * FROM [Match] WHERE Status = 0 AND Id > @id ORDER BY RoundId ASC ",
                new Dictionary<string, object>
                {
                    {"@id", id},
                    {"@nr", i}
                });
        }

        public SqlQuery AddWinner(int winnerId, int roundId)
        {
            return new SqlQuery("UPDATE [Match] Set WinnerId = @winnerId WHERE RoundId = @roundId", new Dictionary<string, object>
            {
                {"@roundId", roundId},
                {"@winnerId", winnerId}
            });
        }

        public SqlQuery GetMatchesUserWon(int userId, int nrOfItemsToReturn, int? startFrom)
        {
            if (startFrom == null)
                return new SqlQuery(
                    "SELECT TOP(@top) * FROM [Match] WHERE Status = 0 AND WinnerId = @winnerId ORDER BY RoundId DESC",
                    new Dictionary<string, object>
                    {
                        {"@winnerId", userId},
                        {"@top", nrOfItemsToReturn},
                    });
            return new SqlQuery(
                "SELECT TOP(@top) * FROM [Match] WHERE Status = 0 AND WinnerId = @winnerId AND Id > @start ORDER BY RoundId ASC",
                new Dictionary<string, object>
                {
                    {"@winnerId", userId},
                    {"@top", nrOfItemsToReturn},
                    {"@start", startFrom},
                });
        }

        public SqlQuery GetMatcheIdsUserWon(int userId)
        {
            return new SqlQuery("SELECT RoundId FROM [Match] WHERE WinnerId = @winnerId", new Dictionary<string, object>
            {
                {"@winnerId", userId}
            });
        }

        public SqlQuery FindRange(List<int> roundIds, bool includeOpenMatches)
        {
            var ids = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < roundIds.Count; index++)
            {
                ids.Add($"@id{index}");
                dict.Add($"@id{index}", roundIds[index]);
            }

            var idsStr = string.Join(",", ids);

            var queryString = includeOpenMatches
                ? $"SELECT * FROM [Match] WHERE RoundId IN ({idsStr})"
                : $"SELECT * FROM [Match] WHERE Status = 0 AND RoundId IN ({idsStr})";
            return new SqlQuery(queryString, dict);
        }

        public SqlQuery FindRangeByMatchIds(List<int> matchIds, bool includeOpenMatches)
        {
            var ids = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < matchIds.Count; index++)
            {
                ids.Add($"@id{index}");
                dict.Add($"@id{index}", matchIds[index]);
            }
            var idsStr = string.Join(",", ids);

            var queryString = includeOpenMatches
                ? $"SELECT * FROM [Match] WHERE Id IN ({idsStr})"
                : $"SELECT * FROM [Match] WHERE Status = 0 AND Id IN ({idsStr})";
            return new SqlQuery(queryString, dict);
            
        }
    }
}