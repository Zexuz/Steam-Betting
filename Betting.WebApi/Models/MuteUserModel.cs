namespace Betting.WebApi.Models
{
    public class MuteUserModel
    {
        public string Reason  { get; set; }
        public int    Seconds { get; set; }
        public string SteamId { get; set; }
    }
}