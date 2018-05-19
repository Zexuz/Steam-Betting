using System.Threading.Tasks;
using Betting.Backend.Resources;

namespace Betting.Backend.Services.Interfaces
{
    public interface IDiscordAuthService
    {
        string RedirectUrl { get; }
        string AuthUrl     { get; }

        Task<DiscordConnectionResource[]> GetConnections(string accessToken);
        Task<DiscordProfileResource>      GetProfileInfo(string accessToken);
        bool                              IsValid(DiscordConnectionResource connectionResource, string steamId);
        DiscordConnectionResource         GetSteamConnection(DiscordConnectionResource[] connectionResource);
        Task<DiscordAuthResource>         GetDiscordAuthResource(string code, string encodedUrl);
//        Task<DiscordConnectionResource>   GetValidSteamIdFromDiscord(string code, string steamId);
    }
}