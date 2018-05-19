using Betting.Backend.Websockets;
using Betting.WebApi.Websocket.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Betting.WebApi.Websocket.Factories
{
    public class WebSocketSenderFactory : IWebSocketSenderFactory
    {
        private readonly IHubContext<SteamHub>    _steamHubContext;
        private readonly IHubContext<MatchHub>    _matchHubContext;
        private readonly IHubContext<TestHub>     _adminHubContext;
        private readonly IHubContext<BetHub>      _betHubContext;
        private readonly IHubContext<ChatHub>     _chatHubContext;
        private readonly IHubContext<InfoHub>     _infoHubContext;
        private readonly IHubContext<TicketHub>   _ticketHubContext;
        private readonly IHubContext<CoinFlipHub> _coinFlipHubContext;
        private readonly IHubConnectionManager    _hubConnectionManager;

        public WebSocketSenderFactory(
            IHubContext<SteamHub> steamHubContext,
            IHubContext<MatchHub> matchHubContext,
            IHubContext<TestHub> adminHubContext,
            IHubContext<BetHub> betHubContext,
            IHubContext<ChatHub> chatHubContext,
            IHubContext<InfoHub> infoHubContext,
            IHubContext<TicketHub> ticketHubContext,
            IHubContext<CoinFlipHub> coinFlipHubContext,
            IHubConnectionManager hubConnectionManager
        )
        {
            _steamHubContext = steamHubContext;
            _matchHubContext = matchHubContext;
            _adminHubContext = adminHubContext;
            _betHubContext = betHubContext;
            _chatHubContext = chatHubContext;
            _infoHubContext = infoHubContext;
            _ticketHubContext = ticketHubContext;
            _coinFlipHubContext = coinFlipHubContext;
            _hubConnectionManager = hubConnectionManager;
        }

        public ISteamHubConnections GetSteamHubSocketSender()
        {
            var socketSender = new WebSocketSender<SteamHub>(_steamHubContext);
            return CrateHubConnection<SteamHub>(socketSender);
        }

        public ITestHubConnections GetAdminHubSocketSender()
        {
            var socketSender = new WebSocketSender<TestHub>(_adminHubContext);
            return CrateHubConnection<TestHub>(socketSender);
        }

        public IMatchHubConnections GetMatchHubSocketSender()
        {
            var socketSender = new WebSocketSender<MatchHub>(_matchHubContext);
            return CrateHubConnection<MatchHub>(socketSender);
        }

        public IBetHubConnections GetBetHubSocketSender()
        {
            var socketSender = new WebSocketSender<BetHub>(_betHubContext);
            return CrateHubConnection<BetHub>(socketSender);
        }

        public IChatHubConnections GetChatHubSocketSender()
        {
            var socketSender = new WebSocketSender<ChatHub>(_chatHubContext);
            return CrateHubConnection<ChatHub>(socketSender);
        }

        public IInfoHubConnections GetInfoHubSocketSender()
        {
            var socketSender = new WebSocketSender<InfoHub>(_infoHubContext);
            return CrateHubConnection<InfoHub>(socketSender);
        }

        public ITicketHubConnections GetTicketHubSocketSender()
        {
            var socketSender = new WebSocketSender<TicketHub>(_ticketHubContext);
            return CrateHubConnection<TicketHub>(socketSender);
        }

        public ICoinFlipHubConnections GetCoinFlipHubSocketSender()
        {
            var socketSender = new WebSocketSender<CoinFlipHub>(_coinFlipHubContext);
            return CrateHubConnection<CoinFlipHub>(socketSender);
        }

        private HubConnections CrateHubConnection<T>(IWebSocketSender webSocketSender)
        {
            var wrapper = new WebsocketWrapper(
                webSocketSender.SendToUser,
                webSocketSender.SendToRoom,
                webSocketSender.SendToAll,
                webSocketSender.SendToAllExcept
            );
            return new HubConnections(wrapper, typeof(T), _hubConnectionManager);
        }
    }
}