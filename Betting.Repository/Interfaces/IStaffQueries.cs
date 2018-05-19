using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IStaffQueries
    {
        SqlQuery GetStaffFromSteamId(string steamId);
        SqlQuery AddStaff(string steamId, int levelId);
        SqlQuery GetAll();
        SqlQuery Remove(int id);
    }
}