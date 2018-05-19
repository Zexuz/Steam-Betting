using System.Threading.Tasks;
using RpcCommunication;

namespace Betting.Backend.Interfaces
{
    public interface IOfferManager
    {
        Task HandleOffer(OfferStatusRequest offer);
    }
}