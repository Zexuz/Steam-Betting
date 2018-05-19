using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Interfaces;
using Betting.Backend.Websockets;
using Microsoft.AspNetCore.SignalR;

namespace Betting.WebApi.Websocket.Hubs
{
    public class InfoHub : HubBase<InfoHub>
    {
        public static readonly HashSet<string> ConnectedClient = new HashSet<string>();

        public InfoHub(ILogServiceFactory logService, IHubConnectionManager hubConnectionManager) : base(logService, hubConnectionManager)
        {
        }

        public override Task OnConnectedAsync()
        {
            ConnectedClient.Add(Context.ConnectionId);
            Clients.All.InvokeAsync("onOnlineChanged", ConnectedClient.Count);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedClient.Remove(Context.ConnectionId);
            Clients.All.InvokeAsync("onOnlineChanged", ConnectedClient.Count);
            return base.OnDisconnectedAsync(exception);
        }
    }
}