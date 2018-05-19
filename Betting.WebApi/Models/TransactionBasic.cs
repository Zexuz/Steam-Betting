using System;

namespace Betting.WebApi.Models
{
    public class TransactionBasic
    {
        public int       Id           { get; set; }
        public bool      IsPending    => Accepted == null;
        public int       BotId        { get; set; }
        public decimal   TotalValue   { get; set; }
        public bool      IsDeposit    { get; set; }
        public DateTime? Accepted     { get; set; }
        public string    SteamOfferId { get; set; }
        public int       ItemCount    { get; set; }
    }
}