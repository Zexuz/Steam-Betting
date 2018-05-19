using System.Threading.Tasks;

using RpcCommunicationChat;

namespace Betting.Backend.Wrappers.Interfaces
{
    public interface IChatServiceClientWrapper : IGrpcBase
    {
        Task<InsertMessageResponse> InsertMessage(InsertMessageRequest request);
        Task<MessagesResponse>      GetLatestMessages(GetLatestMessagesRequest request);
        Task<MuteUserResponse>      MuteUser(MuteUserRequest request);
        Task<MutedUsersResponse>    GetMutedUsers();
        Task<EmptyMessage>          UnMuteUser(UnMuteUserRequest request);
        Task<MessagesResponse>      GetMessagesOnParams(GetMessagesOnParamsRequest request);
    }
}