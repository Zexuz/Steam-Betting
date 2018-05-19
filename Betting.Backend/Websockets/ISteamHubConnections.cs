using System.Threading.Tasks;
 
using RpcCommunication;

namespace Betting.Backend.Websockets
{
    public interface ISteamHubConnections
    {
        Task SendOfferStatusToUser(OfferStatusRequest status, string steamId);
        Task SendErrorMessageRelatedToOurApi(string msg, string steamId);
    }
}