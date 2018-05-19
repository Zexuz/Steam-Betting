
using System.Threading.Tasks;
using RpcCommunicationTicket;

namespace Betting.Backend.Websockets
{
    public interface ITicketHubConnections
    {
        Task TicketUpdate(Ticket ticket);
    }
}