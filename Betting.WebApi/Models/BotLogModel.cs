namespace Betting.WebApi.Models
{
    public class SteamLogModels
    {
        public class BotLog : Bot
        {
            public int? Page       { get; set; }
            public int? StatusCode { get; set; }
        }

        public class LoginLog : Bot
        {
        }

        public class OfferLog : Bot
        {
            public int?   Page        { get; set; }
            public int?   StatusCode  { get; set; }
            public string OfferId     { get; set; }
            public string UserSteamId { get; set; }
        }

        public class ExceptionLog
        {
            public int? Page { get; set; }
        }


        public class Bot
        {
            public string Name    { get; set; }
            public string SteamId { get; set; }
            public string BotType { get; set; }
        }
    }
}