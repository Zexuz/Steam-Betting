using Betting.Backend.Interfaces;
using Betting.Backend.Websockets;

namespace Betting.WebApi.Websocket.Hubs
{
    public class TicketHub : HubBase<TicketHub>
    {
        public TicketHub(ILogServiceFactory logService, IHubConnectionManager hubConnectionManager) : base(logService, hubConnectionManager)
        {
        }
    }
}