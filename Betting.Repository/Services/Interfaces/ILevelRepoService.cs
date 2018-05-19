using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface ILevelRepoService
    {
        Task<List<DatabaseModel.Level>> GetAll();
        Task<DatabaseModel.Level>       Find(int id);
        Task<DatabaseModel.Level>       Add(DatabaseModel.Level level);
        Task<int>                       Remove(int id);
    }
}