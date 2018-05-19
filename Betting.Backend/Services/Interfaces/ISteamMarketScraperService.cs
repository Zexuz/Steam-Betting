using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface ISteamMarketScraperService
    {
        int                         PageSize { get; set; }
        Task<List<SteamMarketItem>> Scrape(int appId);
    }
}