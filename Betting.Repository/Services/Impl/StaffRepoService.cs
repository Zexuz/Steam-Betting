using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Services.Impl
{
    public class StaffRepoService : IStaffRepoService
    {
        private readonly IStaffQueries       _staffQueries;
        private          IDatabaseConnection _databaseConnection;

        public StaffRepoService(IDatabaseConnectionFactory databaseConnectionFactory, IStaffQueries staffQueries)
        {
            _staffQueries       = staffQueries;
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Settings);
        }

        public async Task<DatabaseModel.Staff> Find(string steamId)
        {
            var query = _staffQueries.GetStaffFromSteamId(steamId);
            var res   = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetSingleAsync<DatabaseModel.Staff>();
        }

        public async Task<int> Add(string steamId, int levelId)
        {
            var query = _staffQueries.AddStaff(steamId, levelId);
            return (int) await _databaseConnection.ExecuteScalarAsync(query);
        }

        public async Task<List<DatabaseModel.Staff>> GetAll()
        {
            var query = _staffQueries.GetAll();
            var res   = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetListAsync<DatabaseModel.Staff>();
        }

        public async Task<int> Remove(int id)
        {
            var query = _staffQueries.Remove(id);
            return await _databaseConnection.ExecuteNonQueryAsync(query);
        }
    }
}