using System.Threading.Tasks;

namespace Betting.Backend.Websockets
{
    public interface ITestHubConnections
    {
        Task SendMessageToUser(string steamId, string message);
    }
}