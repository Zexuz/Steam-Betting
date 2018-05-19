using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Betting.Repository.Services.Interfaces
{
    public interface IGameModeRepoService
    {
        Task<DatabaseModel.GameMode>       Insert(DatabaseModel.GameMode mode);
        Task<DatabaseModel.GameMode>       Find(int id);
        Task<DatabaseModel.GameMode>       Find(string type);
        Task<List<DatabaseModel.GameMode>> Find(List<int> ids);
    }

    public class GameModeRepoService : DapperRepoBase, IGameModeRepoService
    {
        public GameModeRepoService(IDatabaseConnectionFactory factory) : base(factory)
        {
        }

        public async Task<DatabaseModel.GameMode> Insert(DatabaseModel.GameMode mode)
        {
            using (var cn = DatabaseConnection.GetNewOpenConnection())
            {
                await cn.InsertAsync(mode);
                cn.Close();
            }

            return mode;
        }

        public async Task<DatabaseModel.GameMode> Find(int id)
        {
            using (var cn = DatabaseConnection.GetNewOpenConnection())
            {
                var res = await cn.GetAsync<DatabaseModel.GameMode>(id);
                cn.Close();
                return res;
            }
        }

        public async Task<DatabaseModel.GameMode> Find(string type)
        {
            using (var cn = DatabaseConnection.GetNewOpenConnection())
            {
                var mode = await cn.QueryAsync<DatabaseModel.GameMode>("SELECT * FROM GameMode WHERE Type = @type", new {Type = type});
                cn.Close();
                return mode.SingleOrDefault();
            }
        }

        public async Task<List<DatabaseModel.GameMode>> Find(List<int> ids)
        {
            using (var cn = DatabaseConnection.GetNewOpenConnection())
            {
                var res = await cn.QueryAsync<DatabaseModel.GameMode>("SELECT * FROM GameMode WHERE Id in @Ids", new {Ids = ids});
                cn.Close();
                return res.ToList();
            }
        }
    }
}