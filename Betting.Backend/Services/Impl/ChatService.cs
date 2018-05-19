using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Factories;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Backend.Wrappers.Interfaces;
using Betting.Repository.Factories;
using RpcCommunicationChat;

namespace Betting.Backend.Services.Impl
{
    public class ChatService : IChatService
    {
        private readonly IRepoServiceFactory       _repoServiceFactory;
        private readonly IChatServiceClientWrapper _chatServiceClient;
        private readonly IChatHubConnections       _chatHubConnections;
        private readonly ISettingsService          _settingsService;
        private readonly IStaffService _staffService;
        private readonly IDiscordService _discordService;

        public ChatService
        (
            IRepoServiceFactory repoServiceFactory,
            IGrpcServiceFactory grpcServiceFactory,
            IChatHubConnections chatHubConnections,
            ISettingsService settingsService,
            IStaffService staffService,
            IDiscordService discordService
        )
        {
            _repoServiceFactory = repoServiceFactory;
            _chatServiceClient  = grpcServiceFactory.GetChatSercviceClient();
            _chatHubConnections = chatHubConnections;
            _settingsService    = settingsService;
            _staffService = staffService;
            _discordService = discordService;
        }

        public async Task<List<ChatMessageModel>> GetLatestMessages()
        {
            var currentSetting = await _settingsService.GetCurrentSettings();
            var res            = await _chatServiceClient.GetLatestMessages(new GetLatestMessagesRequest
            {
                MessagesAmount = currentSetting.NrOfLatestChatMessages
            });

            return res.ChatMessage.Select(message => new ChatMessageModel
            {
                ImageUrl   = message.Image,
                TimeStamp  = message.Timestamp,
                Name       = message.Name,
                UserType   = message.UserType,
                SteamId    = message.SteamId,
                Message    = message.Message
            }).ToList();
        }

        public async Task SendMessage(string message, string steamId)
        {
            var user = await _repoServiceFactory.UserRepoService.FindAsync(steamId);
            var staffName = await _staffService.GetUserStaffName(steamId);
            
            var userStatusName = staffName ?? "User";
            var ignoreRules = staffName != null;

            var reguset = new InsertMessageRequest
            {
                AntiSpamSeconds = 5,
                ChatMessage = new ChatMessage
                {
                    Message         = message,
                    SteamId         = steamId,
                    Image           = user.ImageUrl,
                    Name            = user.Name,
                    UserType = userStatusName
                },
                IgnoreRules = ignoreRules
            };
            var res = await _chatServiceClient.InsertMessage(reguset);
            switch (res.DataCase)
            {
                case InsertMessageResponse.DataOneofCase.ChatMessage:
                    await _chatHubConnections.MessageReceived(new ChatMessageModel
                    {
                        Message    = message,
                        SteamId    = steamId,
                        ImageUrl   = user.ImageUrl,
                        Name       = user.Name,
                        TimeStamp = res.ChatMessage.Timestamp,
                        UserType = res.ChatMessage.UserType
                    });
                    _discordService.ChatMessageAsync(user.Name,message);
                    break;
                case InsertMessageResponse.DataOneofCase.None:
                case InsertMessageResponse.DataOneofCase.Error:
                    await _chatHubConnections.SendError(res.Error.Message, steamId);
                    break;
                default:
                    await _chatHubConnections.SendError(res.Error.Message, steamId);
                    break;
            }
        }

        public async Task<MutedUsersResponse> GetMutedUsers()
        {
            return await _chatServiceClient.GetMutedUsers();
        }

        public async Task<MuteUserResponse> MuteUser(string reason, int seconds, string steamId)
        {
            return await _chatServiceClient.MuteUser(new MuteUserRequest
            {
                Reason  = reason,
                Seconds = seconds,
                SteamId = steamId
            });
        }

        public async Task UnmuteUser(string steamId)
        {
            await _chatServiceClient.UnMuteUser(new UnMuteUserRequest {SteamId = steamId});
        }
        
        public async Task<MessagesResponse> GetMessagesOnParams(string startTime,string endTime, string steamId)
        {
            return await _chatServiceClient.GetMessagesOnParams(new GetMessagesOnParamsRequest
            {
                EndTime = endTime,
                StartTime = startTime,
                SteamId = steamId
            });
        }
    }
}