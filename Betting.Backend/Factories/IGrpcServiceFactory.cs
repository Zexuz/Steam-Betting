using Betting.Backend.Cache;
using Betting.Backend.Wrappers.Interfaces;

namespace Betting.Backend.Factories
{
    public interface IGrpcServiceFactory
    {
        ISteamServiceClientWrapper  GetSteamServiceClient(ISteamInventoryCacheManager cacheManager);
        IChatServiceClientWrapper   GetChatSercviceClient();
        ITicketServiceClientWrapper GetTicketSercviceClient();
        IDiscordServiceClientWrapper GetDiscordSercviceClient();
        IHistoryServiceClientWrapper GetHistorySercviceClient();
    }
}