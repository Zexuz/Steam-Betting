using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Services.Impl
{
    public class LevelRepoService : ILevelRepoService
    {
        private readonly ILevelQueries       _levelQueries;
        private          IDatabaseConnection _databaseConnection;

        public LevelRepoService(IDatabaseConnectionFactory databaseConnectionFactory, ILevelQueries levelQueries)
        {
            _levelQueries       = levelQueries;
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Settings);
        }

        public async Task<List<DatabaseModel.Level>> GetAll()
        {
            var query = _levelQueries.GetAll();
            var res   = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetListAsync<DatabaseModel.Level>();
        }

        public async Task<DatabaseModel.Level> Find(int id)
        {
            var query = _levelQueries.GetLevelFromId(id);
            var res   = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetSingleAsync<DatabaseModel.Level>();
        }

        public async Task<DatabaseModel.Level> Add(DatabaseModel.Level level)
        {
            var query    = _levelQueries.CreteNewLevel(level);
            var res      = await _databaseConnection.ExecuteScalarAsync(query);
            var insertId = (int) res;
            return new DatabaseModel.Level(level.Name, level.Chat, level.Ticket, level.Admin, insertId);
        }

        public async Task<int> Remove(int id)
        {
            var query = _levelQueries.Remove(id);
            return await _databaseConnection.ExecuteNonQueryAsync(query);
        }
    }
}