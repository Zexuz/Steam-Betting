using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;

namespace Betting.Backend.Services.Impl
{
    public class LevelService : ILevelService
    {
        private readonly IRepoServiceFactory _repoServiceFactory;

        public LevelService(IRepoServiceFactory repoServiceFactory)
        {
            _repoServiceFactory = repoServiceFactory;
        }

        public async Task<DatabaseModel.Level> Add(DatabaseModel.Level level)
        {
            return await _repoServiceFactory.LevelRepoService.Add(level);
        }

        public async Task<List<DatabaseModel.Level>> GetAll()
        {
            return await _repoServiceFactory.LevelRepoService.GetAll();
        }

        public async Task<DatabaseModel.Level> Find(int id)
        {
            return await _repoServiceFactory.LevelRepoService.Find(id);
        }

        public async Task<int> Remove(int id)
        {
            return await _repoServiceFactory.LevelRepoService.Remove(id);
        }
    }
}