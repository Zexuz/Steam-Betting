using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface IItemService
    {
        Task<List<Item>> GetAvalibleItemsForUser(DatabaseModel.User user);
        Task<decimal>    GetSumOfItems(List<DatabaseModel.Item> items);
    }
}