using Betting.Backend.Interfaces;
using Betting.Backend.Websockets;

namespace Betting.WebApi.Websocket.Hubs
{
    public class SteamHub : HubBase<SteamHub>
    {
        public SteamHub(ILogServiceFactory logService, IHubConnectionManager hubConnectionManager) : base(logService, hubConnectionManager)
        {
        }
    }
}