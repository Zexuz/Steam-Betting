using Betting.Backend.Factories;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Wrappers.Interfaces;
using RpcCommunicationDiscord;
using Exception = System.Exception;

namespace Betting.Backend.Services.Impl
{
    public class DiscordService : IDiscordService
    {
        private readonly ILogService<DiscordService>  _logger;
        public           IDiscordAuthService          AuthService { get; }
        private readonly IDiscordServiceClientWrapper _discrodClientWrapper;

        public DiscordService(IDiscordAuthService discordAuthService, IGrpcServiceFactory grpcServiceFactory, ILogServiceFactory logger)
        {
            _logger = logger.CreateLogger<DiscordService>();
            AuthService = discordAuthService;
            _discrodClientWrapper = grpcServiceFactory.GetDiscordSercviceClient();
        }

        public async void SendPersonalMessageAsync(string message, string toSteamId)
        {
            await _discrodClientWrapper.SendPersonalMessageAsync(new PersonalMessageRequest
            {
                Message = message,
                SteamId = toSteamId
            });
        }

        public async void AddUserAsync(string id, string steamId)
        {
            await _discrodClientWrapper.AddUserAsync(new AddUserRequest
            {
                Id = id,
                SteamId = steamId
            });
        }

        public async void ChatMessageAsync(string name, string message)
        {
            await _discrodClientWrapper.ChatMessageAsync(new ChatMessageRequest
            {
                Request = new ChatMessageMessage
                {
                    Message = message,
                    Name = name
                }
            });
        }

        public async void CoinFlipCreateAsync(bool csgo, bool pubg, decimal value, string coinFlipId, string userId)
        {
            await _discrodClientWrapper.CoinFlipCreateAsync(new CoinFlipCreateRequest
            {
                Request = new CoinFlipCreateMessage
                {
                    AllowCsgo = csgo,
                    AllowPubg = pubg,
                    CoinFlipId = coinFlipId,
                    UserId = userId,
                    Value = (double) value
                }
            });
        }

        public async void CoinFlipJoinAsync(decimal value, string coinFlipId, string userId)
        {
            await _discrodClientWrapper.CoinFlipCreateJoin(new CoinFlipJoinRequest
            {
                Request = new CoinFlipJoinMessage
                {
                    CoinFlipId = coinFlipId,
                    UserId = userId,
                    Value = (double) value,
                }
            });
        }

        public async void CoinFlipWinnerAsync(string coinFlipId, decimal totalValue)
        {
            await _discrodClientWrapper.CoinFlipWinnerAsync(new CoinFlipWinnerRequest
            {
                Request = new CoinFlipWinnerMessage
                {
                    CoinFlipId = coinFlipId,
                    TotalValue = (double) totalValue,
                }
            });
        }

        public async void GlobalExceptionErrorAsync(string corelcationId, string location, Exception e, string userId)
        {
            await _discrodClientWrapper.GlobalExceptionErrorAsync(new GlobalExceptionErrorRequest
            {
                Error = new ExceptionMessage
                {
                    CorelcationId = corelcationId,
                    ExceptionMessage_ = e.Message,
                    Location = location,
                    UserId = userId
                }
            });
        }

        public async void JackpotBetAsync(int roundId, string userId, decimal value)
        {
            await _discrodClientWrapper.JackpotBetAsync(new JackpotBetRequest
            {
                Request = new JackpotBetMessage
                {
                    RoundId = roundId.ToString(),
                    UserId = userId,
                    Value = (double) value
                }
            });
        }

        public async void JackpotWinnerAsync(int roundId, decimal value)
        {
            await _discrodClientWrapper.JackpotWinnerAsync(new JackpotWinnerRequest
            {
                Request = new JackpotWinnerMessage
                {
                    RoundId = roundId.ToString(),
                    Value = (double) value
                }
            });
        }

//        private async Task SendMessageToAdmins(string msg)
//        {
//            var adminsSteamIds = new List<string>
//            {
//                "76561198077954112",
//                "76561198171028263",
//                "76561198033067954"
//            };
//
//            try
//            {
//                foreach (var adminsSteamId in adminsSteamIds)
//                {
//                    await SendPersonalMessageAsync(msg, adminsSteamId);
//                }
//            }
//            catch (System.Exception e)
//            {
//                _logger.Error(null, null, e, new Dictionary<string, object>
//                {
//                    {"msg", msg}
//                });
//            }
//        }
    }
}