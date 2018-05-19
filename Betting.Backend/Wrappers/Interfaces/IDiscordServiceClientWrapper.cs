using System.Threading.Tasks;
 
using RpcCommunicationDiscord;

namespace Betting.Backend.Wrappers.Interfaces
{
    public interface IDiscordServiceClientWrapper : IGrpcBase
    {
        Task                          AddUserAsync(AddUserRequest request);
        Task<PersonalMessageResponse> SendPersonalMessageAsync(PersonalMessageRequest request);
        Task<EmptyResponse>           ChatMessageAsync(ChatMessageRequest request);
        Task<EmptyResponse>           CoinFlipCreateAsync(CoinFlipCreateRequest request);
        Task<EmptyResponse>           CoinFlipCreateJoin(CoinFlipJoinRequest request);
        Task<EmptyResponse>           CoinFlipWinnerAsync(CoinFlipWinnerRequest request);
        Task<EmptyResponse>           GlobalExceptionErrorAsync(GlobalExceptionErrorRequest request);
        Task<EmptyResponse>           JackpotBetAsync(JackpotBetRequest request);
        Task<EmptyResponse>           JackpotWinnerAsync(JackpotWinnerRequest request);
    }
}