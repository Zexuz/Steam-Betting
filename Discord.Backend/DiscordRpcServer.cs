using System.Threading.Tasks;
using Grpc.Core;
using RpcCommunicationDiscord;

namespace Discord.Backend
{
    public class DiscordRpcServer : DiscordService.DiscordServiceBase
    {
        public event AddUserDelegate        OnAddUser;
        public event PingDelegate           OnPing;
        public event TicketResponseDelegate OnTicketResponse;


        public event ChatMessageDelegate     OnChatMessage;
        public event CoinFlipCreateDelegate  OnCoinFlipCreate;
        public event CoinFlipJoinDelegate    OnCoinFlipJoin;
        public event CoinFlipWinnerDelegate  OnCoinFlipWinner;
        public event JackpotWinnerDelegate   OnJackpotWinner;
        public event JackpotBetDelegate      OnJackpotBet;
        public event GlobalExceptionDelegate OnGlobalException;
        public event UserLoginDelegate       OnUserLogin;

        public delegate void AddUserDelegate(object sender, AddUserRequest request);

        public delegate void PingDelegate(object sender, PingRequest request);

        public delegate void TicketResponseDelegate(object sender, PersonalMessageRequest request);

        public delegate void ChatMessageDelegate(object sender, ChatMessageRequest request);

        public delegate void CoinFlipCreateDelegate(object sender, CoinFlipCreateRequest request);

        public delegate void CoinFlipJoinDelegate(object sender, CoinFlipJoinRequest request);

        public delegate void CoinFlipWinnerDelegate(object sender, CoinFlipWinnerRequest request);

        public delegate void JackpotWinnerDelegate(object sender, JackpotWinnerRequest request);

        public delegate void JackpotBetDelegate(object sender, JackpotBetRequest request);

        public delegate void GlobalExceptionDelegate(object sender, GlobalExceptionErrorRequest request);

        public delegate void UserLoginDelegate(object sender, UserLoginRequest request);


        public override Task<PersonalMessageResponse> SendPersonalMessage(PersonalMessageRequest request, ServerCallContext context)
        {
            OnTicketResponse?.Invoke(this, request);
            return Task.FromResult(new PersonalMessageResponse());
        }

        public override Task<AddUserResponse> AddUser(AddUserRequest request, ServerCallContext context)
        {
            OnAddUser?.Invoke(this, request);
            return Task.FromResult(new AddUserResponse());
        }

        public override Task<RpcCommunicationDiscord.PingResponse> Ping(RpcCommunicationDiscord.PingRequest request, ServerCallContext context)
        {
            OnPing?.Invoke(this, new PingRequest());
            return Task.FromResult(new RpcCommunicationDiscord.PingResponse());
        }

        public override Task<EmptyResponse> ChatMessage(ChatMessageRequest request, ServerCallContext context)
        {
            OnChatMessage?.Invoke(this, request);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> CoinFlipCreate(CoinFlipCreateRequest request, ServerCallContext context)
        {
            OnCoinFlipCreate?.Invoke(this, request);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> CoinFlipJoin(CoinFlipJoinRequest request, ServerCallContext context)
        {
            OnCoinFlipJoin?.Invoke(this, request);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> CoinFlipWinner(CoinFlipWinnerRequest request, ServerCallContext context)
        {
            OnCoinFlipWinner?.Invoke(this, request);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> GlobalExceptionError(GlobalExceptionErrorRequest request, ServerCallContext context)
        {
            OnGlobalException?.Invoke(this, request);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> JackpotBet(JackpotBetRequest request, ServerCallContext context)
        {
            OnJackpotBet?.Invoke(this, request);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> JackpotWinner(JackpotWinnerRequest request, ServerCallContext context)
        {
            OnJackpotWinner?.Invoke(this, request);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> UserLogin(UserLoginRequest request, ServerCallContext context)
        {
            OnUserLogin?.Invoke(this, request);
            return Task.FromResult(new EmptyResponse());
        }
    }
}