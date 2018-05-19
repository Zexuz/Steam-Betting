using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Betting.Repository.Services.Impl
{
    public class CoinFlipMatchRepoService : ICoinFlipMatchRepoService
    {
        private readonly IDatabaseConnection _connection;

        public CoinFlipMatchRepoService(IDatabaseConnectionFactory connection)
        {
            _connection = connection.GetDatabaseConnection(Database.Main);
        }


        public async Task<DatabaseModel.CoinFlip> InsertAsync(DatabaseModel.CoinFlip coinFlip, ITransactionWrapper transactionWrapper)
        {
            var cn = transactionWrapper.SqlConnection;
            var transaction = transactionWrapper.Transaction;

            var insertedId = await cn.InsertAsync(coinFlip, transaction);
            coinFlip.Id = insertedId;
            return coinFlip;
        }

        public async Task UpdateAsync(DatabaseModel.CoinFlip coinflip)
        {
            using (var cn = _connection.GetNewOpenConnection())
            {
                await cn.UpdateAsync(coinflip);
            }
        }

        public async Task<DatabaseModel.CoinFlip> FindAsync(int id)
        {
            using (var cn = _connection.GetNewOpenConnection())
            {
                var res = await cn.QuerySingleOrDefaultAsync<DatabaseModel.CoinFlip>("SELECT * FROM CoinFlip WHERE Id = @Id", new {Id = id});
                return res;
            }
        }

        public async Task<List<DatabaseModel.CoinFlip>> FindAllOpenMatchesAsync()
        {
            using (var cn = _connection.GetNewOpenConnection())
            {
                var res = await cn.QueryAsync<DatabaseModel.CoinFlip>("SELECT * FROM CoinFlip WHERE Status = @status", new
                {
                    Status = (int) MatchStatus.Open
                });
                return res.ToList();
            }
        }

        public async Task<List<DatabaseModel.CoinFlip>> FindAllOpenMatchesAsync(DatabaseModel.User user)
        {
            using (var cn = _connection.GetNewOpenConnection())
            {
                var res = await cn.QueryAsync<DatabaseModel.CoinFlip>("SELECT * FROM CoinFlip WHERE Status = @status AND CreatorUserId = @userId", new
                {
                    Status = (int) MatchStatus.Open,
                    UserId = user.Id
                });
                return res.ToList();
            }
        }

        public async Task<List<DatabaseModel.CoinFlip>> FindAllNotClosedMatches()
        {
            using (var cn = _connection.GetNewOpenConnection())
            {
                var res = await cn.QueryAsync<DatabaseModel.CoinFlip>("SELECT * FROM CoinFlip WHERE Status != @closedStatus", new
                {
                    ClosedStatus = (int) MatchStatus.Closed
                });
                return res.ToList();
            }
        }

        public async Task RemoveAsync(DatabaseModel.CoinFlip coinFlip)
        {
            using (var cn = _connection.GetNewOpenConnection())
            {
                await cn.ExecuteAsync("DELETE FROM CoinFlip WHERE Id = @Id", new {Id = coinFlip.Id});
            }
        }
    }
}