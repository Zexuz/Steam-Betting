using System.Threading.Tasks;
using Betting.Backend.Interfaces;
using Betting.Backend.Wrappers.Interfaces;
 
using RpcCommunicationChat;

namespace Betting.Backend.Wrappers
{
    public class ChatServiceClientWrapper : GrpcClientWrapperBase<ChatServiceClientWrapper>, IChatServiceClientWrapper
    {
        private readonly ChatService.ChatServiceClient _chatServiceClient;

        public ChatServiceClientWrapper
        (
            ChatService.ChatServiceClient chatServiceClient,
            ILogServiceFactory logServiceFactory
        )
            : base(logServiceFactory.CreateLogger<ChatServiceClientWrapper>())
        {
            _chatServiceClient = chatServiceClient;
        }

        public async Task<InsertMessageResponse> InsertMessage(InsertMessageRequest request)
        {
            return await SendGrpcAction(async () => await _chatServiceClient.InsertMessageAsync(request, DefaultSettings.GetDefaultSettings(5)));
        }

        public async Task<MessagesResponse> GetLatestMessages(GetLatestMessagesRequest request)
        {
            return await SendGrpcAction(async () => await _chatServiceClient.GetLatestMessagesAsync(request, DefaultSettings.GetDefaultSettings(5)));
        }

        public async Task<MuteUserResponse> MuteUser(MuteUserRequest request)
        {
            return await SendGrpcAction(async () => await _chatServiceClient.MuteUserAsync(request, DefaultSettings.GetDefaultSettings(5)));
        }

        public async Task<MutedUsersResponse> GetMutedUsers()
        {
            return await SendGrpcAction(async () => await _chatServiceClient.GetMutedUsersAsync(new EmptyMessage(), DefaultSettings.GetDefaultSettings(5)));
        }

        public async Task<EmptyMessage> UnMuteUser(UnMuteUserRequest request)
        {
            return await SendGrpcAction(async () => await _chatServiceClient.UnMuteUserAsync(request, DefaultSettings.GetDefaultSettings(5)));
        }

        public async Task<MessagesResponse> GetMessagesOnParams(GetMessagesOnParamsRequest request)
        {
            return await SendGrpcAction(async () =>
                await _chatServiceClient.GetMessagesOnParamsAsync(request, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task PingAsync()
        {
            return SendGrpcAction(async () => await _chatServiceClient.PingAsync(new EmptyMessage(), DefaultSettings.GetDefaultSettings(2)));
        }
    }
}