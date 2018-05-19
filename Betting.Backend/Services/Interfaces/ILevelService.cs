using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface ILevelService
    {
        Task<DatabaseModel.Level>       Add(DatabaseModel.Level level);
        Task<List<DatabaseModel.Level>> GetAll();
        Task<DatabaseModel.Level>       Find(int id);
        Task<int>                       Remove(int id);
    }
}