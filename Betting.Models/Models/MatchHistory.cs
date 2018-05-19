using System;

namespace Betting.Models.Models
{
    public class MatchHistory
    {
        public int      RoundId       { get; set; }
        public string   Salt          { get; set; }
        public string   Hash          { get; set; }
        public string   Percentage    { get; set; }
        public DateTime Created       { get; set; }
        public string   WinnerSteamId { get; set; }
        public decimal  MatchValue    { get; set; }
        public decimal  UserValue     { get; set; }
        public int      ItemsInMatch  { get; set; }
    }
}