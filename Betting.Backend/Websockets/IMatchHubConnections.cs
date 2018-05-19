using System.Threading.Tasks;
using Betting.Backend.Websockets.Models;
using Betting.Models.Models;

namespace Betting.Backend.Websockets
{
    public interface IMatchHubConnections
    {
        Task UserBetsOnMatch(UserBetsOnMatchModel obj);
        Task MatchIsClosed(int roundId);
        Task NewMatchCreated(JackpotMatch jackpotMatch);
        Task Winner(JackpotWinnerSelected jackpotWinnerSelected);
    }
}