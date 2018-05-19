using Betting.Backend.Interfaces;
using Betting.Backend.Websockets;

namespace Betting.WebApi.Websocket.Hubs
{
    public class BetHub : HubBase<BetHub>
    {
        public BetHub(ILogServiceFactory logService,IHubConnectionManager hubConnectionManager) : base(logService,hubConnectionManager)
        {
        }
    }
}