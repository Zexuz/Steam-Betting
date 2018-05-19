using System.Threading.Tasks;
using Discord.Backend;
using Discord.Backend.Enum;
using Discord.WebSocket;
using Grpc.Core;
 
using RpcCommunicationDiscord;

namespace Discord.Runner
{
    public class GrpcServerService
    {
        private readonly DiscordSocketClient _client;
        private readonly SubscribeManager    _subscribeManager;

        public GrpcServerService(DiscordSocketClient client, SubscribeManager subscribeManager)
        {
            _client = client;
            _subscribeManager = subscribeManager;
        }

        public void StartGrpcServer()
        {
            var discordServiceServer = new DiscordRpcServer();

            discordServiceServer.OnAddUser += DiscordServiceServerOnAddUser;
            discordServiceServer.OnTicketResponse += DiscordServiceServerOnTicketResponse;
            discordServiceServer.OnJackpotWinner += DiscordServiceServerOnJackpotWinner;
            discordServiceServer.OnJackpotBet += DiscordServiceServerOnJackpotBet;
            discordServiceServer.OnGlobalException += DiscordServiceServerOnGlobalException;
            discordServiceServer.OnCoinFlipWinner += DiscordServiceServerOnCoinFlipWinner;
            discordServiceServer.OnCoinFlipJoin += DiscordServiceServerOnCoinFlipJoin;
            discordServiceServer.OnCoinFlipCreate += DiscordServiceServerOnCoinFlipCreate;
            discordServiceServer.OnChatMessage += DiscordServiceServerOnChatMessage;

            var server = new Server
            {
                Services = {DiscordService.BindService(discordServiceServer)},
                Ports = {new ServerPort("localhost", 50056, ServerCredentials.Insecure)} //todo secure this connection
            };

            server.Start();
        }

        private async void DiscordServiceServerOnChatMessage(object sender, ChatMessageRequest request)
        {
            var @event = Event.ChatMessage;
            var msg = $"{request.Request.Name}: {request.Request.Message}";

            await SendMessageToUserSubscribed(@event, msg);
        }

        private async void DiscordServiceServerOnCoinFlipCreate(object sender, CoinFlipCreateRequest request)
        {
            var @event = Event.CoinFlipCreate;
            var msg = $"{request.Request.UserId} CREATED COINFLIP, Value: {request.Request.Value}: {request.Request.CoinFlipId}";

            await SendMessageToUserSubscribed(@event, msg);
        }

        private async void DiscordServiceServerOnCoinFlipJoin(object sender, CoinFlipJoinRequest request)
        {
            var @event = Event.CoinFlipJoin;
            var msg = $"{request.Request.UserId} JOINED COINFLIP, Value: {request.Request.Value}: {request.Request.CoinFlipId}";

            await SendMessageToUserSubscribed(@event, msg);
        }

        private async void DiscordServiceServerOnCoinFlipWinner(object sender, CoinFlipWinnerRequest request)
        {
            var @event = Event.CoinFlipWinner;
            var msg = $"{request.Request.CoinFlipId} ended with Value: {request.Request.TotalValue}";

            await SendMessageToUserSubscribed(@event, msg);
        }

        private async void DiscordServiceServerOnGlobalException(object sender, GlobalExceptionErrorRequest request)
        {
            var @event = Event.GlobalExceptionError;
            var msg = $"{nameof(request.Error.CorelcationId)}:{request.Error.CorelcationId} \n " +
                      $"Message: {request.Error.ExceptionMessage_} \n "                          +
                      $"Location: {request.Error.Location}"                                      +
                      $"UserId: {request.Error.UserId}";

            await SendMessageToUserSubscribed(@event, msg);
        }

        private async void DiscordServiceServerOnJackpotBet(object sender, JackpotBetRequest request)
        {
            var @event = Event.JackpotUserBetted;
            var msg = $"{request.Request.UserId} JOINED JACKPOT, Value: {request.Request.Value}: on roundId: {request.Request.RoundId}";

            await SendMessageToUserSubscribed(@event, msg);
        }

        private async void DiscordServiceServerOnJackpotWinner(object sender, JackpotWinnerRequest request)
        {
            var @event = Event.JackpotWinner;
            var msg = $"{request.Request.RoundId} ended with a value of {request.Request.Value}";

            await SendMessageToUserSubscribed(@event, msg);
        }

        private async void DiscordServiceServerOnTicketResponse(object sender, PersonalMessageRequest request)
        {
            var @event = Event.TicketCrated;
            var msg = $"{request.SteamId} responded on ticket";

            await SendMessageToUserSubscribed(@event, msg);
        }

        private async void DiscordServiceServerOnAddUser(object sender, AddUserRequest request)
        {
            var @event = Event.UserLoginFirstTime;
            var msg = $"{request.SteamId} loggin in for the first time";

            await SendMessageToUserSubscribed(@event, msg);
        }

        private async Task SendMessageToUserSubscribed(Event @event, string msg)
        {
            var users = _subscribeManager.GetUsersSubscribedToEvent(@event);
            foreach (var user in users)
            {
                await _client.GetUser(user).SendMessageAsync($"{@event.ToString()}: {msg}");
            }
        }
    }
}