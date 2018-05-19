using Betting.Backend.Interfaces;
using Betting.Backend.Websockets;

namespace Betting.WebApi.Websocket.Hubs
{
    public class TestHub : HubBase<TestHub>
    {
        public TestHub(ILogServiceFactory factory, IHubConnectionManager hubConnectionManager) : base(factory, hubConnectionManager)
        {
        }
    }
}