using System;
using System.Threading.Tasks;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.WebApi.Extensions;
using Microsoft.AspNetCore.SignalR;
using Serilog.Core.Trackers;

namespace Betting.WebApi.Websocket.Hubs
{
    public abstract class HubBase<T> : Hub
    {
        private readonly IHubConnectionManager   _hubConnectionManager;
        private readonly ILogService<HubBase<T>> _logService;

        public HubBase(ILogServiceFactory logService, IHubConnectionManager hubConnectionManager)
        {
            _hubConnectionManager = hubConnectionManager;
            _logService = logService.CreateLogger<HubBase<T>>();
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var steamId = Context.User.GetSteamId();
                await Groups.AddAsync(Context.Connection.ConnectionId, steamId);
                _hubConnectionManager.Add(typeof(T),new HubConnectionManager.ConnectionAndSteamId
                {
                    ConnectionId = Context.Connection.ConnectionId,
                    SteamId = steamId
                });
            }
            catch (System.Exception e)
            {
                // ignored
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userId = "Not logged in user";
            try
            {
                userId = Context.User.GetSteamId();
            }
            catch (Exception)
            {
                // ignored
            }

            if (exception != null)
                new ErrorTracker(userId, "hubBase", "DomainName", "WebSocket", exception, Guid.NewGuid().ToString()).Stop();
            try
            {
                var steamId = Context.User.GetSteamId();
                await Groups.RemoveAsync(Context.Connection.ConnectionId, steamId);
                _hubConnectionManager.Remove(typeof(T),new HubConnectionManager.ConnectionAndSteamId
                {
                    ConnectionId = Context.Connection.ConnectionId,
                    SteamId = steamId
                });
            }
            catch (Exception e)
            {
                // ignored
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}