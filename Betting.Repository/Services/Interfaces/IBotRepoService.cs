using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IBotRepoService
    {
        Task<DatabaseModel.Bot>       InsertAsync(DatabaseModel.Bot bot);
        Task<DatabaseModel.Bot>       FindAsync(string steamId);
        Task<DatabaseModel.Bot>       FindAsync(int id);
        Task<List<DatabaseModel.Bot>> FindAsync(List<int> ids);
        Task<List<DatabaseModel.Bot>> GetAll();
    }
}