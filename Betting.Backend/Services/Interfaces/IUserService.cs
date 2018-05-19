using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Services.Impl;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> UserLoggedIn(DatabaseModel.User user);
//        Task<List<MatchHistory>> GetMatchesUserLost(DatabaseModel.User user, int nrOfItems, int? from);
        Task<List<MatchHistory>>        GetMatchesUserWon(DatabaseModel.User user, int nrOfItems, int? from);
        Task<WinsAndLosses>             GetMatchHistoryForUser(DatabaseModel.User user, int limit, int? from);
        Task<bool> UpdateUserInfoIfNeeded(DatabaseModel.User newUserInfo, DatabaseModel.User user);
        Task<DatabaseModel.User> FindAsync(string steamId);
    }
}