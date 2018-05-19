using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface ILevelQueries
    {
        SqlQuery GetLevelFromId(int id);
        SqlQuery CreteNewLevel(DatabaseModel.Level level);
        SqlQuery GetAll();
        SqlQuery Remove(int id);
    }
}