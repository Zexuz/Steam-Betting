using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;

namespace Betting.Backend.Services.Impl
{
    public class StaffService : IStaffService
    {
        private readonly IRepoServiceFactory _repoServiceFactory;
        private readonly ILevelService       _levelService;

        public StaffService(IRepoServiceFactory repoServiceFactory, ILevelService levelService)
        {
            _repoServiceFactory = repoServiceFactory;
            _levelService       = levelService;
        }
        
        public async Task<string> GetUserStaffName(string steamId)
        {
            var staff = await _repoServiceFactory.StaffRepoService.Find(steamId);
            if (staff == null) return null;
            var level = await _levelService.Find(staff.Level);

            return level.Name;
        }


        public async Task<bool> IsAdmin(string steamId)
        {
            var staff = await _repoServiceFactory.StaffRepoService.Find(steamId);
            if (staff == null) return false;
            var level = await _levelService.Find(staff.Level);

            return level.Admin;
        }

        public async Task<bool> CanHandleTicket(string steamId)
        {
            var staff = await _repoServiceFactory.StaffRepoService.Find(steamId);
            if (staff == null) return false;
            var level = await _levelService.Find(staff.Level);

            return level.Ticket;
        }

        public async Task<bool> CanModChat(string steamId)
        {
            var staff = await _repoServiceFactory.StaffRepoService.Find(steamId);
            if (staff == null) return false;
            var level = await _levelService.Find(staff.Level);

            return level.Chat;
        }

        public async Task<List<DatabaseModel.Staff>> GetAll()
        {
            return await _repoServiceFactory.StaffRepoService.GetAll();
        }

        public async Task<DatabaseModel.Staff> Add(DatabaseModel.Staff staff)
        {
            var id = await _repoServiceFactory.StaffRepoService.Add(staff.SteamId, staff.Level);
            return new DatabaseModel.Staff(staff.SteamId, staff.Level, id);
        }

        public async Task<int> Remove(int id)
        {
            return await _repoServiceFactory.StaffRepoService.Remove(id);
        }
    }
}