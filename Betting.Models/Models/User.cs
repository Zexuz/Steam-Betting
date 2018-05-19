namespace Betting.Models.Models
{
    public class User
    {
        public string SteamId  { get; set; }
        public string Name     { get; set; }
        public string ImageUrl { get; set; }
    }
    
    public class UserWithQuote:User
    {
        public string Quote { get; set; }
    }
}