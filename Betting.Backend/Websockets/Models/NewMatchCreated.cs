namespace Betting.Backend.Websockets.Models
{
    public class NewMatchCreated
    {
        public int    Id              { get; set; }
        public int    ItemLimit       { get; set; }
        public double TimerLengthInMs { get; set; }
    }
}