using System;
using System.Collections.Generic;
using System.Linq;

namespace Betting.Models.Models
{
    public class JackpotMatchHistoryBasic
    {
        public int      RoundId           { get; set; }
        public string   Salt              { get; set; }
        public string   Hash              { get; set; }
        public string   Percentage        { get; set; }
        public DateTime Created           { get; set; }
        public string   UserWinnerName    { get; set; }
        public string   UserWinnerImgUrl  { get; set; }
        public string   UserWinnerQuote   { get; set; }
        public string   UserWinnerSteamId { get; set; }
    }

    public class JackpotMatchHistoryDetailed : JackpotMatchHistoryBasic
    {
        public decimal    ValueInPool => ItemsInPool.Sum(item => item.Value);
        public List<Bet>  Bets        { get; set; }
        public List<Item> ItemsInPool => Bets.SelectMany(bet => bet.Items).ToList();
    }
}