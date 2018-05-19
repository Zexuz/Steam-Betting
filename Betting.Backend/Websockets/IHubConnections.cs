using System.Collections.Generic;
using System.Threading.Tasks;

namespace Betting.Backend.Websockets
{
    public interface IHubConnections
    {
        Task SendToUser(string msg, string userRoom, string methodName);
        Task SendToRoom(string msg, string userRoom, string methodName);
        Task SendToAll(string msg, string methodName);
        Task SendToAllExcept(string msg, string methodName, IReadOnlyList<string> exludedUser);
    }
}