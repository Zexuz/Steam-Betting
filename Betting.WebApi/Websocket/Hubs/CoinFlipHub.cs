using Betting.Backend.Interfaces;
using Betting.Backend.Websockets;

namespace Betting.WebApi.Websocket.Hubs
{
    public class CoinFlipHub : HubBase<CoinFlipHub>
    {
        public CoinFlipHub(ILogServiceFactory logService, IHubConnectionManager hubConnectionManager) : base(logService, hubConnectionManager)
        {
        }
    }
}