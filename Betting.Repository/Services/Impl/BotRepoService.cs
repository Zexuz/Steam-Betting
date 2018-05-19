using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Services.Impl
{
    public class BotRepoService : IBotRepoService
    {
        private readonly IDatabaseConnection _databaseConnection;
        private readonly IBotQueries         _botQueries;

        public BotRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IBotQueries botQueries)
        {
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Main);
            _botQueries         = botQueries;
        }

        public async Task<DatabaseModel.Bot> InsertAsync(DatabaseModel.Bot bot)
        {
            var query = _botQueries.InsertReturnsId(bot);
            var id    = (int) await _databaseConnection.ExecuteScalarAsync(query);
            return new DatabaseModel.Bot(bot.SteamId, bot.Name, id);
        }

        public async Task<DatabaseModel.Bot> FindAsync(string steamId)
        {
            var query = _botQueries.GetFromSteamId(steamId);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.Bot>();
            }
        }

        public async Task<DatabaseModel.Bot> FindAsync(int id)
        {
            var query = _botQueries.GetFromId(id);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetSingleAsync<DatabaseModel.Bot>();
            }
        }

        public async Task<List<DatabaseModel.Bot>> FindAsync(List<int> ids)
        {
            if (ids.Count == 0)
                return new List<DatabaseModel.Bot>();

            var query = _botQueries.GetFromIds(ids);
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Bot>();
            }
        }

        public async Task<List<DatabaseModel.Bot>> GetAll()
        {
            var query = _botQueries.GetAll();
            using (var sqlResult = await _databaseConnection.ExecuteSqlQueryAsync(query))
            {
                return await sqlResult.GetListAsync<DatabaseModel.Bot>();
            }
        }
    }
}