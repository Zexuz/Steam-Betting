using System;
using System.Collections.Generic;

namespace Betting.Models.Models
{
    public class Bet
    {
        public User        User     { get; set; }
        public DateTime    DateTime { get; set; }
        public RoundTicket Tickets  { get; set; }
        public List<Item>  Items    { get; set; }
    }

    public class CoinFlipBet
    {
        public User        User     { get; set; }
        public bool        IsHead   { get; set; }
        public DateTime    DateTime { get; set; }
        public RoundTicket Tickets  { get; set; }
        public List<Item>  Items    { get; set; }
    }

    public class RoundTicket
    {
        public int Start { get; set; }
        public int End   { get; set; }
    }
}