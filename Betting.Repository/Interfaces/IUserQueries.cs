using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IUserQueries
    {
        SqlQuery InsertReturnsId(DatabaseModel.User user);
        SqlQuery GetAllUsersAsync();
        SqlQuery GetUserWithSteamId(string steamId);
        SqlQuery InsertAsync(DatabaseModel.User user);
        SqlQuery UpdateUsersName(string steamId, string newName);
        SqlQuery UpdateUsersTradeLink(string steamId, string newTradelink);
        SqlQuery UpdateUsersImage(string steamId, string newImage);
        SqlQuery UpdateUsersNameAndImage(string steamId, string newName, string newImage);
        SqlQuery Delete(string steamId);
        SqlQuery GetUserWithId(int id);
        SqlQuery GetUsersWithId(List<int> ids);
        SqlQuery UpdateUsersQuote(string steamId, string quote);
        SqlQuery GetUsersWithSteamIds(List<string> steamId);
    }
}