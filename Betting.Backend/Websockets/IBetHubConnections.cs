using System.Threading.Tasks;

namespace Betting.Backend.Websockets
{
    public interface IBetHubConnections
    {
        Task Error(string steamId, int roundId, string msg);
        Task Success(string steamId, int roundId);
    }
}