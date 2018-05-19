using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface IUserRepoService
    {
        Task<DatabaseModel.User>       InsertAsync(DatabaseModel.User user);
        Task<DatabaseModel.User>       InsertAsync(string steamId, string name, string imgUrl);
        Task<DatabaseModel.User>       FindAsync(string steamId);
        Task<DatabaseModel.User>       FindAsync(int id);
        Task                           UpdateImageAsync(string steamId, string imageUrl);
        Task                           UpdateNameAsync(string steamId, string name);
        Task                           UpdateImageAndNameAsync(string steamId, string name, string imageUrl);
        Task                           UpdateTradelinkAsync(string steamId, string tradelink);
        Task<List<DatabaseModel.User>> FindAsync(List<int> ids);
        Task                           UpdateQuoteAsync(string steamId, string quote);
        Task<List<DatabaseModel.User>> FindAsync(List<string> steamId);
        Task<List<DatabaseModel.User>> FindAsync(DateTime start, DateTime end);
    }
}