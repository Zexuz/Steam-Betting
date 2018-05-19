using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;
using Dapper;

namespace Betting.Repository.Services.Impl
{
    public class BetRepoService : IBetRepoService
    {
        private readonly IBetQueries         _betQueries;
        private readonly IDatabaseConnection _databaseConnection;

        public BetRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IBetQueries betQueries)
        {
            _betQueries = betQueries;
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
        }

        public async Task<List<DatabaseModel.Bet>> FindAsync(DatabaseModel.User user)
        {
            var query = _betQueries.GetAllBetsFromUser(user.Id);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Bet>();
            }
        }

        public async Task<List<DatabaseModel.Bet>> FindAsync(DatabaseModel.User user, int limit, int? from)
        {
            var query = _betQueries.GetBetsFromUser(user.Id, limit, from);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Bet>();
            }
        }

        public async Task<List<int>> FindAsync(DatabaseModel.User user, List<int> matchIds, int gameModeId)
        {
            if (matchIds.Count == 0) return new List<int>();

            var query = _betQueries.GetBetIdsFromUserAndMatch(user, matchIds, gameModeId);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var list = await sqlResult.GetListAsync<DatabaseModel.Bet>();
                return list.Select(l => l.Id).ToList();
            }
        }

        public async Task<List<DatabaseModel.Bet>> FindAsync(DatabaseModel.Match match)
        {
            var query = _betQueries.GetAllBetsForMatch(match.Id, match.GameModeId);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Bet>();
            }
        }

        public async Task<List<DatabaseModel.Bet>> FindAsync(int matchId, int gameModeId)
        {
            var query = _betQueries.GetAllBetsForMatch(matchId, gameModeId);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Bet>();
            }
        }

        public async Task<DatabaseModel.Bet> FindAsync(DatabaseModel.Match match, DatabaseModel.User user)
        {
            var query = _betQueries.GetBet(match.Id, user.Id, match.GameModeId);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.Bet>();
            }
        }

        public async Task<List<DatabaseModel.Bet>> FindAsync(List<LookUpGameModeBet> lookUpGameModeBets)
        {
            using (var cn = _databaseConnection.GetNewOpenConnection())
            {
                var res = new List<DatabaseModel.Bet>();

                foreach (var lookUpGameModeBet in lookUpGameModeBets)
                {
                    var callRes = await cn.QueryAsync<DatabaseModel.Bet>(
                        "SELECT * FROM [Bet] WHERE GameModeId = @gameModeId AND UserId = @userId AND MatchId IN @matchIds", new
                        {
                            GameModeId = lookUpGameModeBet.GameMode.Id,
                            UserId = lookUpGameModeBet.User.Id,
                            MatchIds = lookUpGameModeBet.MatchIds
                        });
                    res.AddRange(callRes);
                }

                return res.ToList();
            }
        }

        public async Task<DatabaseModel.Bet> InsertAsync(DatabaseModel.Bet bet, ITransactionWrapper transactionWrapper = null)
        {
            var query = _betQueries.InsertReturnsId(bet);

            if (transactionWrapper != null)
            {
                var idFromTrans = await transactionWrapper.ExecuteSqlCommand<int>(query);
                return new DatabaseModel.Bet(bet.UserId, bet.MatchId, bet.GameModeId, bet.Created, idFromTrans);
            }

            var id = (int) await _databaseConnection.ExecuteScalarAsync(query);
            return new DatabaseModel.Bet(bet.UserId, bet.MatchId, bet.GameModeId, bet.Created, id);
        }
    }
}