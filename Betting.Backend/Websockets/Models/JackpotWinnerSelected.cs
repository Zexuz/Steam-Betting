namespace Betting.Backend.Websockets.Models
{
    public class JackpotWinnerSelected
    {
        public string  SteamId          { get; set; }
        public string  Name             { get; set; }
        public string  ImageUrl         { get; set; }
        public string  Percentage       { get; set; }
        public string  Quote            { get; set; }
        public decimal PotValue         { get; set; }
        public int     RoundId          { get; set; }
        public string  DraftingGraph    { get; set; }
        public int     DraftingTimeInMs { get; set; }
    }
}