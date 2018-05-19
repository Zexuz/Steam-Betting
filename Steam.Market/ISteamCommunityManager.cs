using System.Threading.Tasks;
using Steam.Market.Models.Resources;

namespace Steam.Market
{
    public interface ISteamCommunityManager
    {
        bool UserIsLoggedIn { get; }
        Task<PriceHistoryResource> GetPriceHistoryResource(int appId, string marketHashName);
        Task<bool>                 Login();
    }
}