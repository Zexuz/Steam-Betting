using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Websockets;
using RpcCommunicationChat;

namespace Betting.Backend.Services.Interfaces
{
    public interface IChatService
    {
        Task<List<ChatMessageModel>> GetLatestMessages();
        Task                         SendMessage(string message, string steamId);
        Task<MutedUsersResponse>     GetMutedUsers();
        Task<MuteUserResponse>       MuteUser(string reason, int seconds, string steamId);
        Task                         UnmuteUser(string steamId);
        Task<MessagesResponse> GetMessagesOnParams(string startTime, string endTime, string steamId);
    }
}