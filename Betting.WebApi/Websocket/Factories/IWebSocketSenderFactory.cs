using Betting.Backend.Websockets;

namespace Betting.WebApi.Websocket.Factories
{
    public interface IWebSocketSenderFactory
    {
        ISteamHubConnections    GetSteamHubSocketSender();
        ITestHubConnections     GetAdminHubSocketSender();
        IMatchHubConnections    GetMatchHubSocketSender();
        IBetHubConnections      GetBetHubSocketSender();
        IChatHubConnections     GetChatHubSocketSender();
        IInfoHubConnections     GetInfoHubSocketSender();
        ITicketHubConnections   GetTicketHubSocketSender();
        ICoinFlipHubConnections GetCoinFlipHubSocketSender();
    }
}