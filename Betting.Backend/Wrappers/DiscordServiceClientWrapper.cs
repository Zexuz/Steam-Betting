using System.Threading.Tasks;
using Betting.Backend.Interfaces;
using Betting.Backend.Wrappers.Interfaces;
 
using RpcCommunicationDiscord;

namespace Betting.Backend.Wrappers
{
    public class DiscordServiceClientWrapper : GrpcClientWrapperBase<DiscordServiceClientWrapper>, IDiscordServiceClientWrapper
    {
        private readonly DiscordService.DiscordServiceClient _discordService;

        public DiscordServiceClientWrapper
        (
            DiscordService.DiscordServiceClient discordService,
            ILogServiceFactory logServiceFactory
        )
            : base(logServiceFactory.CreateLogger<DiscordServiceClientWrapper>())
        {
            _discordService = discordService;
        }

        public Task AddUserAsync(AddUserRequest request)
        {
            return SendGrpcAction(async () => await _discordService.AddUserAsync(request, DefaultSettings.GetDefaultSettings(2)), false);
        }

        public Task PingAsync()
        {
            return SendGrpcAction(async () => await _discordService.PingAsync(new RpcCommunicationDiscord.PingRequest(), DefaultSettings.GetDefaultSettings(2)), true);
        }

        public Task<PersonalMessageResponse> SendPersonalMessageAsync(PersonalMessageRequest request)
        {
            return SendGrpcAction(async () => await _discordService.SendPersonalMessageAsync(request, DefaultSettings.GetDefaultSettings(2)), false);
        }

        public Task<EmptyResponse> ChatMessageAsync(ChatMessageRequest request)
        {
            return SendGrpcAction(async () => await _discordService.ChatMessageAsync(request, DefaultSettings.GetDefaultSettings(2)), false);
        }

        public Task<EmptyResponse> CoinFlipCreateAsync(CoinFlipCreateRequest request)
        {
            return SendGrpcAction(async () => await _discordService.CoinFlipCreateAsync(request, DefaultSettings.GetDefaultSettings(2)), false);
        }

        public Task<EmptyResponse> CoinFlipCreateJoin(CoinFlipJoinRequest request)
        {
            return SendGrpcAction(async () => await _discordService.CoinFlipJoinAsync(request, DefaultSettings.GetDefaultSettings(2)), false);
        }

        public Task<EmptyResponse> CoinFlipWinnerAsync(CoinFlipWinnerRequest request)
        {
            return SendGrpcAction(async () => await _discordService.CoinFlipWinnerAsync(request, DefaultSettings.GetDefaultSettings(2)), false);
        }

        public Task<EmptyResponse> GlobalExceptionErrorAsync(GlobalExceptionErrorRequest request)
        {
            return SendGrpcAction(async () => await _discordService.GlobalExceptionErrorAsync(request, DefaultSettings.GetDefaultSettings(2)), false);
        }

        public Task<EmptyResponse> JackpotBetAsync(JackpotBetRequest request)
        {
            return SendGrpcAction(async () => await _discordService.JackpotBetAsync(request, DefaultSettings.GetDefaultSettings(2)), false);
        }

        public Task<EmptyResponse> JackpotWinnerAsync(JackpotWinnerRequest request)
        {
            return SendGrpcAction(async () => await _discordService.JackpotWinnerAsync(request, DefaultSettings.GetDefaultSettings(2)), false);
        }
    }
}