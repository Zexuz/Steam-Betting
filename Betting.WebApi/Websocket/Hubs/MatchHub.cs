using Betting.Backend.Interfaces;
using Betting.Backend.Websockets;

namespace Betting.WebApi.Websocket.Hubs
{
    public class MatchHub : HubBase<MatchHub>
    {
        public MatchHub(ILogServiceFactory logService, IHubConnectionManager hubConnectionManager) : base(logService, hubConnectionManager)
        {
        }
    }
}