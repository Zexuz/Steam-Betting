using Betting.Models.Models;

namespace Betting.Backend.Websockets.Models
{
    public class CoinFlipWinnerSelected
    {
        public UserWithQuote Winner        { get; set; }
        public int           WinningTicket { get; set; }
        public string        RoundId       { get; set; }
        public string        Percentage    { get; set; }
        public decimal       PotValue      { get; set; }
        public string        Salt          { get; set; }
    }
}