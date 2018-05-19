using System;

namespace Betting.Models.Models
{
    public class JackpotMatchSetting
    {
        public decimal  Rake                { get;set;}
        public TimeSpan TimmerLength        { get;set;}
        public int      ItemLimit           { get;set;}
        public int      MaxItemAUserCanBet  { get;set;}
        public int      MinItemAUserCanBet  { get;set;}
        public decimal  MaxValueAUserCanBet { get;set;}
        public decimal  MinValueAUserCanBet { get;set;}
        public TimeSpan DraftingTime        { get;set;}
        public string   DraftingGraph       { get;set;}
        public bool     AllowCsgo           { get;set;}
        public bool     AllowPubg           { get;set;}

        public JackpotMatchSetting
        (
            decimal rake,
            TimeSpan timmerLength,
            int itemLimit,
            int maxItemAUserCanBet,
            int minItemAUserCanBet,
            decimal maxValueAUserCanBet,
            decimal minValueAUserCanBet,
            bool allowCsgo,
            bool allowPubg,
            TimeSpan draftingTime,
            string draftingGraph
        )
        {
            Rake = rake;
            TimmerLength = timmerLength;
            ItemLimit = itemLimit;
            MaxItemAUserCanBet = maxItemAUserCanBet;
            MinItemAUserCanBet = minItemAUserCanBet;
            MaxValueAUserCanBet = maxValueAUserCanBet;
            MinValueAUserCanBet = minValueAUserCanBet;
            DraftingTime = draftingTime;
            DraftingGraph = draftingGraph;
            AllowCsgo = allowCsgo;
            AllowPubg = allowPubg;
        }
    }
}