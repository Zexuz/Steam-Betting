using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Websockets.Models;
using Betting.Models.Models;
using RpcCommunication;
using Shared.Shared.Web;
using Ticket = RpcCommunicationTicket.Ticket;

namespace Betting.Backend.Websockets
{
    public class WebsocketWrapper
    {
        public Func<string, string, string, Task>                SendToUser      { get; private set; }
        public Func<string, string, string, Task>                SendToRoom      { get; private set; }
        public Func<string, string, Task>                        SendToAll       { get; private set; }
        public Func<string, string, IReadOnlyList<string>, Task> SendToAllExcept { get; private set; }

        public WebsocketWrapper(
            Func<string, string, string, Task> sendToUser,
            Func<string, string, string, Task> sendToRoom,
            Func<string, string, Task> sendToAll,
            Func<string, string, IReadOnlyList<string>, Task> sendToAllExcept
        )
        {
            SendToUser = sendToUser;
            SendToRoom = sendToRoom;
            SendToAll = sendToAll;
            SendToAllExcept = sendToAllExcept;
        }
    }

    public class HubConnections :
        ISteamHubConnections,
        ITestHubConnections,
        IMatchHubConnections,
        IBetHubConnections,
        IChatHubConnections,
        IInfoHubConnections,
        ITicketHubConnections,
        ICoinFlipHubConnections
    {
        private readonly WebsocketWrapper      _websocketWrapper;
        private readonly Type                  _type;
        private readonly IHubConnectionManager _hubConnectionManager;

        public HubConnections(WebsocketWrapper websocketWrapper, Type type, IHubConnectionManager hubConnectionManager)
        {
            _websocketWrapper = websocketWrapper;
            _type = type;
            _hubConnectionManager = hubConnectionManager;
        }

        #region ISteamHub

        public async Task SendOfferStatusToUser(OfferStatusRequest status, string steamId)
        {
            await SendToUser(status, steamId, "onOfferStatus");
        }

        public async Task SendErrorMessageRelatedToOurApi(string msg, string steamId)
        {
            await SendToUser(msg, steamId, "onOfferStatusError");
        }

        #endregion

        #region IMatchHub

        public async Task UserBetsOnMatch(UserBetsOnMatchModel obj)
        {
            await SendToAll(obj, "onBetPlaced");
        }

        public async Task MatchIsClosed(int roundId)
        {
            await SendToAll(roundId, "onClosed");
        }

        public async Task NewMatchCreated(JackpotMatch jackpotMatch)
        {
            await SendToAll(jackpotMatch, "onCreated");
        }

        public async Task Winner(JackpotWinnerSelected jackpotWinnerSelected)
        {
            await SendToAll(jackpotWinnerSelected, "onWinner");
        }

        #endregion

        #region IBetHub

        public async Task Error(string steamId, int roundId, string msg)
        {
            await SendToUser(msg, steamId, "error");
        }

        public async Task Success(string steamId, int roundId)
        {
            await SendToUser(roundId, steamId, "success");
        }

        #endregion

        #region ITestHub

        public async Task SendMessageToUser(string steamId, string message)
        {
            await SendToUser(message, steamId, "test");
        }

        #endregion

        #region IChatHub

        public async Task MessageReceived(ChatMessageModel message)
        {
            await SendToAll(message, "onChatMessage");
        }

        public async Task SendError(string message, string steamId)
        {
            await SendToUser(message, steamId, "onError");
        }

        #endregion

        #region ITickethub

        public async Task TicketUpdate(Ticket ticket)
        {
            await SendToUser(ticket, ticket.SteamId, "onTicketResponse");
        }

        #endregion

        #region ICoinFlip

        public async Task MatchDrafting(CoinFlipWinnerSelected match)
        {
            await SendToAll(match, "onWinnerSelected");
        }

        public async Task MatchClosed(DatabaseModel.CoinFlip match)
        {
            await SendToAll(match.RoundId, "onMatchDone");
        }

        public async Task MatchCreated(CoinFlipMatch match)
        {
            await SendToAll(match, "onMatchCreated");
        }

        public async Task MatchTimmerStarted(TimerStartedWebsocketModel model)
        {
            await SendToAll(model, "onTimerStarted");
        }

        public async Task MatchIsNoLongerHot(string roundId)
        {
            await SendToAll(roundId, "onMatchNoLongerHot");
        }

        public async Task MatchIsHot(string roundId, string steamId)
        {
            await SendToAllExcept(roundId, "onMatchIsHot", new[] {steamId});
        }

        #endregion

        #region private methods

        private async Task SendToUser(object msg, string userRoom, string methodName)
        {
            var json = JsonHelper.GetJsonStringFromObjcet(msg);
            await _websocketWrapper.SendToUser(json, userRoom, methodName);
        }

        private async Task SendToRoom(object msg, string userRoom, string methodName)
        {
            var json = JsonHelper.GetJsonStringFromObjcet(msg);
            await _websocketWrapper.SendToRoom(json, userRoom, methodName);
        }

        private async Task SendToAll(object msg, string methodName)
        {
            var json = JsonHelper.GetJsonStringFromObjcet(msg);
            await _websocketWrapper.SendToAll(json, methodName);
        }

        private async Task SendToAllExcept(object msg, string methodName, IReadOnlyList<string> exludedUser)
        {
            var json = JsonHelper.GetJsonStringFromObjcet(msg);
            try
            {
                var excludedUserConnectionIds = exludedUser.SelectMany(steamId => _hubConnectionManager.Get(_type, steamId)).ToList();
                await _websocketWrapper.SendToAllExcept(json, methodName, excludedUserConnectionIds);
            }
            catch (System.Exception )
            {
                await _websocketWrapper.SendToAllExcept(json, methodName, new List<string>());
            }
        }

        #endregion
    }
}