using Betting.Models.Models;

namespace Betting.Backend.Websockets.Models
{
    public class UserBetsOnMatchModel
    {
        public int MatchId { get; set; }
        public Bet Bet     { get; set; }
    }
}