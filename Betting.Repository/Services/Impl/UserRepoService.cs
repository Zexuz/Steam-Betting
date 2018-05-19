using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Exceptions;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;
using Dapper;

namespace Betting.Repository.Services.Impl
{
    public class UserRepoService : IUserRepoService
    {
        private readonly IDatabaseConnection _databaseConnection;
        private readonly IUserQueries        _userQueries;

        public UserRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IUserQueries userQueries)
        {
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
            _userQueries = userQueries;
        }

        public async Task<DatabaseModel.User> InsertAsync(DatabaseModel.User user)
        {
            var query = _userQueries.InsertReturnsId(user);
            var id = (int) await _databaseConnection.ExecuteScalarAsync(query);
            return new DatabaseModel.User(user.SteamId, user.Name, user.ImageUrl, user.TradeLink, user.Created, user.LastActive,
                user.SuspendedFromQuote, user.Quote, id);
        }

        public async Task<DatabaseModel.User> InsertAsync(string steamId, string name, string imgUrl)
        {
            var user = new DatabaseModel.User(steamId, name, imgUrl, null, DateTime.Now, DateTime.Now, false);
            return await InsertAsync(user);
        }

        public async Task<DatabaseModel.User> FindAsync(string steamId)
        {
            var query = _userQueries.GetUserWithSteamId(steamId);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var user = await sqlRes.GetSingleAsync<DatabaseModel.User>();
                return user;
            }
        }

        public async Task<DatabaseModel.User> FindAsync(int id)
        {
            var query = _userQueries.GetUserWithId(id);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                var user = await sqlRes.GetSingleAsync<DatabaseModel.User>();
                return user;
            }
        }

        public async Task UpdateImageAsync(string steamId, string imageUrl)
        {
            var query = _userQueries.UpdateUsersImage(steamId, imageUrl);
            var affectedRows = await _databaseConnection.ExecuteNonQueryAsync(query);
            if (affectedRows != 1)
                throw new NoneExpectedResultException($"Expected the row match count to be one, it was {affectedRows}");
        }

        public async Task UpdateNameAsync(string steamId, string name)
        {
            var query = _userQueries.UpdateUsersName(steamId, name);
            var affectedRows = await _databaseConnection.ExecuteNonQueryAsync(query);
            if (affectedRows != 1)
                throw new NoneExpectedResultException($"Expected the row match count to be one, it was {affectedRows}");
        }

        public async Task UpdateImageAndNameAsync(string steamId, string name, string imageUrl)
        {
            var query = _userQueries.UpdateUsersNameAndImage(steamId, name, imageUrl);
            var affectedRows = await _databaseConnection.ExecuteNonQueryAsync(query);
            if (affectedRows != 1)
                throw new NoneExpectedResultException($"Expected the row match count to be one, it was {affectedRows}");
        }

        public async Task UpdateTradelinkAsync(string steamId, string tradelink)
        {
            var query = _userQueries.UpdateUsersTradeLink(steamId, tradelink);
            var affectedRows = await _databaseConnection.ExecuteNonQueryAsync(query);
            if (affectedRows != 1)
                throw new NoneExpectedResultException($"Expected the row match count to be one, it was {affectedRows}");
        }

        public async Task<List<DatabaseModel.User>> FindAsync(List<int> ids)
        {
            if (ids.Count == 0) return new List<DatabaseModel.User>();
            var query = _userQueries.GetUsersWithId(ids);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlRes.GetListAsync<DatabaseModel.User>();
            }
        }

        public async Task UpdateQuoteAsync(string steamId, string quote)
        {
            var query = _userQueries.UpdateUsersQuote(steamId, quote);

            if ((await FindAsync(steamId)).SuspendedFromQuote)
            {
                throw new UserSuspendedFromUpdatingQuoteException("I told you I would do it!");
            }

            var affectedRows = await _databaseConnection.ExecuteNonQueryAsync(query);
            if (affectedRows != 1)
                throw new NoneExpectedResultException($"Expected the row match count to be one, it was {affectedRows}");
        }

        public async Task<List<DatabaseModel.User>> FindAsync(List<string> steamId)
        {
            if (steamId.Count == 0) return new List<DatabaseModel.User>();
            var query = _userQueries.GetUsersWithSteamIds(steamId);
            using (var sqlRes = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlRes.GetListAsync<DatabaseModel.User>();
            }
        }

        public async Task<List<DatabaseModel.User>> FindAsync(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End can not be before start", nameof(end));


            using (var cn = _databaseConnection.GetNewOpenConnection())
            {
                var users = await cn.QueryAsync<DatabaseModel.User>("SELECT * FROM [User] WHERE Created > @Start AND Created < @End",
                    new {Start = start, End = end});
                return users.ToList();
            }
        }
    }
}