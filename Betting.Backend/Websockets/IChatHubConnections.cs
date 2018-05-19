using System.Threading.Tasks;

namespace Betting.Backend.Websockets
{
    public interface IChatHubConnections
    {
        Task MessageReceived(ChatMessageModel message);
        Task SendError(string message, string steamId);
    }

    public class ChatMessageModel
    {
        public string Message   { get; set; }
        public string SteamId   { get; set; }
        public string Name      { get; set; }
        public string ImageUrl  { get; set; }
        public string TimeStamp { get; set; }
        public string UserType  { get; set; }
    }
}