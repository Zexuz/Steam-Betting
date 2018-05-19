using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services
{
    public interface IMongoPreHashRepoService
    {
        Task Insert(MongoDbModels.PreHash hash);
        Task<MongoDbModels.PreHash> Find(string hash, string steamId);
    }
}