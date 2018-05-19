using System.Threading.Tasks;

namespace Betting.Backend.Wrappers.Interfaces
{
    public interface IGrpcBase
    {
        Task PingAsync();
    }
}