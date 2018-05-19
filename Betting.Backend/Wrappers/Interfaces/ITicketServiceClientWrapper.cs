using System.Threading.Tasks;
 
using RpcCommunicationTicket;

namespace Betting.Backend.Wrappers.Interfaces
{
    public interface ITicketServiceClientWrapper:IGrpcBase
    {
        Task<SingleTicketResponse>           UserCreateTicket(UserCreateTicketRequest reg);
        Task<SingleTicketResponse>           UserRespondToTicket(UserRespondToTicketRequest req);
        Task<ListTicketsResponse>            UserGetAllTickets(Steamid steamId);
        Task<UserCountUnreadTicketsResponse> UserCountUnreadTickets(Steamid steamId);
        Task<SingleTicketResponse>           UserMarkTicketAsRead(UserMarkTicketAsReadRequest reg);


        Task<SingleTicketResponse> AdminCreateTicket(AdminCreateTicketRequest req);
        Task<SingleTicketResponse> AdminRespondToTicket(AdminRespondToTicketRequest req);
        Task<SingleTicketResponse> AdminChangeStatusOnTicket(AdminChangeStatusOnTicketRequest req);
        Task<ListTicketsResponse>  AdminGetTicketsOnQuery(AdminGetTicketsOnQueryRequest req);
    }
}