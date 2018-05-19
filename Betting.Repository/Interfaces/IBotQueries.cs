using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IBotQueries
    {
        SqlQuery InsertRange(List<DatabaseModel.Bot> bots);
        SqlQuery InsertReturnsId(DatabaseModel.Bot bot);
        SqlQuery GetAll();
        SqlQuery GetFromId(int id);
        SqlQuery GetFromSteamId(string steamId);
        SqlQuery Insert(DatabaseModel.Bot bot);
        SqlQuery Delete(string steamId);
        SqlQuery GetFromIds(List<int> ids);
    }
}