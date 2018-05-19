using System.Threading.Tasks;
using Betting.Backend.Interfaces;
using Betting.Backend.Wrappers.Interfaces;
 
using RpcCommunicationTicket;

namespace Betting.Backend.Wrappers
{
    public class TicketServiceClientWrapper : GrpcClientWrapperBase<TicketServiceClientWrapper>, ITicketServiceClientWrapper
    {
        private readonly TicketService.TicketServiceClient _ticketServiceClient;

        public TicketServiceClientWrapper(
            TicketService.TicketServiceClient ticketServiceClient,
            ILogServiceFactory logServiceFactory
        )
            : base(logServiceFactory.CreateLogger<TicketServiceClientWrapper>())
        {
            _ticketServiceClient = ticketServiceClient;
        }

        public Task<SingleTicketResponse> UserCreateTicket(UserCreateTicketRequest reg)
        {
            return SendGrpcAction(async () => await _ticketServiceClient.UserCreateTicketAsync(reg, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task<SingleTicketResponse> UserRespondToTicket(UserRespondToTicketRequest req)
        {
            return SendGrpcAction(async () => await _ticketServiceClient.UserRespondToTicketAsync(req, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task<ListTicketsResponse> UserGetAllTickets(Steamid steamId)
        {
            return SendGrpcAction(async () => await _ticketServiceClient.UserGetAllTicketsAsync(steamId, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task<UserCountUnreadTicketsResponse> UserCountUnreadTickets(Steamid steamId)
        {
            return SendGrpcAction(async () => await _ticketServiceClient.UserCountUnreadTicketsAsync(steamId, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task<SingleTicketResponse> UserMarkTicketAsRead(UserMarkTicketAsReadRequest reg)
        {
            return SendGrpcAction(async () => await _ticketServiceClient.UserMarkTicketAsReadAsync(reg, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task<SingleTicketResponse> AdminCreateTicket(AdminCreateTicketRequest req)
        {
            return SendGrpcAction(async () => await _ticketServiceClient.AdminCreateTicketAsync(req, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task<SingleTicketResponse> AdminRespondToTicket(AdminRespondToTicketRequest req)
        {
            return SendGrpcAction(async () => await _ticketServiceClient.AdminRespondToTicketAsync(req, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task<SingleTicketResponse> AdminChangeStatusOnTicket(AdminChangeStatusOnTicketRequest req)
        {
            return SendGrpcAction(async () => await _ticketServiceClient.AdminChangeStatusOnTicketAsync(req, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task<ListTicketsResponse> AdminGetTicketsOnQuery(AdminGetTicketsOnQueryRequest req)
        {
            return SendGrpcAction(async () => await _ticketServiceClient.AdminGetTicketsOnQueryAsync(req, DefaultSettings.GetDefaultSettings(5)));
        }

        public Task PingAsync()
        {
            return SendGrpcAction(async () => await _ticketServiceClient.PingAsync(new EmptyMessage(), DefaultSettings.GetDefaultSettings(2)));
        }
    }
}