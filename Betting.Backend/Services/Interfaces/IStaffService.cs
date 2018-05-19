using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface IStaffService
    {
        Task<bool>                      IsAdmin(string steamId);
        Task<string>                    GetUserStaffName(string steamId);
        Task<bool>                      CanHandleTicket(string steamId);
        Task<bool>                      CanModChat(string steamId);
        Task<List<DatabaseModel.Staff>> GetAll();
        Task<DatabaseModel.Staff>       Add(DatabaseModel.Staff staff);
        Task<int>                       Remove(int id);
    }
}