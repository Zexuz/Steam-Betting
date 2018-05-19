using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;
using Database = Betting.Repository.Factories.Database;

namespace Betting.Repository.Services.Impl
{
    public class MatchRepoService : IMatchRepoService
    {
        private readonly IDatabaseConnection _databaseConnection;
        private readonly IMatchQueries       _matchQueries;

        public MatchRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IMatchQueries matchQueries)
        {
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
            _matchQueries = matchQueries;
        }

        public async Task<DatabaseModel.Match> FindAsync(int roundId)
        {
            var query = _matchQueries.GetMatchFromRoundId(roundId);
            return await ExecuteQueryAndGetSingle(query);
        }

        public async Task<DatabaseModel.Match> GetCurrentMatch()
        {
            try
            {
                var query = _matchQueries.GetCurrentMatch();
                return await ExecuteQueryAndGetSingle(query);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<DatabaseModel.Match> InsertAsync(DatabaseModel.Match match)
        {
            if (match.SettingId <= 0) throw new ArgumentOutOfRangeException("The settingId on a match must be grater than 0");
            using (var cn = _databaseConnection.GetNewOpenConnection())
            {
                await cn.InsertAsync(match);
                cn.Close();
                return match;
            }
        }

        public async Task AddWinnerToMatch(DatabaseModel.User winner, int matchRoundId)
        {
            var query = _matchQueries.AddWinner(winner.Id, matchRoundId);
            await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task StartTimerForMatch(int roundId, DateTime time)
        {
            var query = _matchQueries.AddTimer(roundId, time);
            await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task CloseMatch(int roundId)
        {
            var query = _matchQueries.ChangeMatchStatus(roundId, MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Closed));
            await _databaseConnection.ExecuteNonQueryAsync(query);
        }

        public async Task<List<DatabaseModel.Match>> FindRangeAsync(int id, int i)
        {
            var query = _matchQueries.FindRange(id, i);
            var res = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetListAsync<DatabaseModel.Match>();
        }

        public async Task<List<DatabaseModel.Match>> GetMatchesUserWon(DatabaseModel.User user, int nrOfItems, int? startFrom)
        {
            var query = _matchQueries.GetMatchesUserWon(user.Id, nrOfItems, startFrom);
            var res = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetListAsync<DatabaseModel.Match>();
        }

        public async Task<List<int>> GetMatchIdsUserWon(DatabaseModel.User user)
        {
            var query = _matchQueries.GetMatcheIdsUserWon(user.Id);
            var res = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetListAsync<int>();
        }

        public async Task<List<DatabaseModel.Match>> FindAsync(List<int> roundIds, bool includeOpenMatches = false)
        {
            if (roundIds.Count == 0) return new List<DatabaseModel.Match>();
            var query = _matchQueries.FindRange(roundIds, includeOpenMatches);
            var res = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetListAsync<DatabaseModel.Match>();
        }

        public async Task<List<DatabaseModel.Match>> FindByMatchIdsAsync(List<int> matchIds, bool includeOpenMatches = false)
        {
            if (matchIds.Count == 0) return new List<DatabaseModel.Match>();
            var query = _matchQueries.FindRangeByMatchIds(matchIds, includeOpenMatches);
            var res = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetListAsync<DatabaseModel.Match>();
        }

        public async Task<List<DatabaseModel.Match>> FindAsync(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End can not be before start", nameof(end));

            using (var cn = _databaseConnection.GetNewOpenConnection())
            {
                var matches = await cn.QueryAsync<DatabaseModel.Match>("SELECT * FROM [Match] WHERE Created > @Start AND Created < @End",
                    new {Start = start, End = end});
                return matches.ToList();
            }
        }

        private async Task<DatabaseModel.Match> ExecuteQueryAndGetSingle(SqlQuery query)
        {
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.Match>();
            }
        }
    }
}