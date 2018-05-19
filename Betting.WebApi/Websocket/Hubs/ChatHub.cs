using Betting.Backend.Interfaces;
using Betting.Backend.Websockets;

namespace Betting.WebApi.Websocket.Hubs
{
    public class ChatHub : HubBase<ChatHub>
    {
        public ChatHub(ILogServiceFactory logService, IHubConnectionManager hubConnectionManager) : base(logService, hubConnectionManager)
        {
        }
    }
}