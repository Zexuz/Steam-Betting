using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IStaffRepoService
    {
        Task<DatabaseModel.Staff>       Find(string steamId);
        Task<int>                       Add(string steamId, int levelId);
        Task<List<DatabaseModel.Staff>> GetAll();
        Task<int>                       Remove(int id);
    }
}