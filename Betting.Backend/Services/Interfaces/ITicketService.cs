using System.Threading.Tasks;
using RpcCommunicationTicket;

namespace Betting.Backend.Services.Interfaces
{
    public interface ITicketService
    {
        Task<SingleTicketResponse>           UserCreateTicket(UserCreateTicketRequest req);
        Task<SingleTicketResponse>           UserRespondToTicket(UserRespondToTicketRequest req);
        Task<ListTicketsResponse>            UserGetAllTickets(string steamId);
        Task<UserCountUnreadTicketsResponse> UserCountUnreadTickets(string steamId);
        Task<SingleTicketResponse>           UserMarkTicketAsRead(string steamId, string ticketId);


        Task<SingleTicketResponse> AdminCreateTicket(AdminCreateTicketRequest req);
        Task<SingleTicketResponse> AdminRespondToTicket(AdminRespondToTicketRequest req);
        Task<SingleTicketResponse> AdminChangeStatusOnTicket(AdminChangeStatusOnTicketRequest req);
        Task<ListTicketsResponse>  AdminGetTicketsOnQuery(AdminGetTicketsOnQueryRequest req);
    }
}