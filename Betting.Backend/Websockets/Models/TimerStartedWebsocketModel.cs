using Betting.Models.Models;

namespace Betting.Backend.Websockets.Models
{
    public class TimerStartedWebsocketModel
    {
        public string RoundId                { get; set; }
        public CoinFlipBet Bet                    { get; set; }
        public int    DraftingTimeInMilliSec { get; set; }
    }
}