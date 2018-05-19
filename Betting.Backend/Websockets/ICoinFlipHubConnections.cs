using System.Threading.Tasks;
using Betting.Backend.Websockets.Models;
using Betting.Models.Models;

namespace Betting.Backend.Websockets
{
    public interface ICoinFlipHubConnections
    {
        Task MatchDrafting(CoinFlipWinnerSelected match);
        Task MatchClosed(DatabaseModel.CoinFlip match);
        Task MatchCreated(CoinFlipMatch match);
        Task MatchTimmerStarted(TimerStartedWebsocketModel model);
        Task MatchIsNoLongerHot(string roundId);
        Task MatchIsHot(string roundId, string steamId);
    }
}