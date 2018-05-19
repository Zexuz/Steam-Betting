using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.WebApi.Websocket.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Betting.WebApi.Websocket
{
    public class WebSocketSender<T> : IWebSocketSender where T : HubBase<T>
    {
        private readonly IHubContext<T> _hubContext;

        public WebSocketSender(IHubContext<T> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUser(string msg, string userRoom, string methodName)
        {
            await _hubContext.Clients.Group(userRoom).InvokeAsync(methodName, msg);
        }

        public async Task SendToRoom(string msg, string userRoom, string methodName)
        {
            await _hubContext.Clients.Group(userRoom).InvokeAsync(methodName, msg);
        }

        public async Task SendToAll(string msg, string methodName)
        {
            await _hubContext.Clients.All.InvokeAsync(methodName, msg);
        }

        public async Task SendToAllExcept(string msg, string methodName, IReadOnlyList<string> exludedUser)
        {
            await _hubContext.Clients.AllExcept(exludedUser).InvokeAsync(methodName, msg);
        }
    }
}