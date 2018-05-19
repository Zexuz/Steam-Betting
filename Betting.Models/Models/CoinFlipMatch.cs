using System;
using System.Collections.Generic;
using System.Linq;

namespace Betting.Models.Models
{
    public class CoinFlipMatch
    {
        public int LookUpId { get; }
        public string      RoundId        { get; }
        public string      Hash           { get; }
        public string      Salt           => Status == MatchStatus.Closed ? _salt : null;
        public string      Percentage     => Status == MatchStatus.Closed ? _percentage : null;
        public string      ReadableStatus { get; }
        public MatchStatus Status         { get; }

        public decimal ValueInPool => ItemsInPool.Sum(item => item.Value);

        public double TimeLeft => TimerStarted != null
            ? ((TimerStarted.Value + Setting.TimmerLength) - DateTime.Now).TotalMilliseconds
            : Setting.TimmerLength.TotalMilliseconds;

        public DateTime? TimerStarted { get; }

        public List<CoinFlipBet> Bets { get; }

        public List<Item> ItemsInPool => Bets.SelectMany(bet => bet.Items).ToList();

        public JackpotMatchSetting Setting { get; }


        private readonly string _percentage;
        private readonly string _salt;

        
        public CoinFlipMatch(MongoDbModels.JackpotMatch jackpotMatch) : this
        (
            jackpotMatch.LookUpId,
            jackpotMatch.RoundId,
            jackpotMatch.Hash,
            jackpotMatch.Salt,
            jackpotMatch.Percentage,
            (MatchStatus) jackpotMatch.Status,
            jackpotMatch.Bets,
            jackpotMatch.TimerStarted,
            jackpotMatch.Setting
        )
        {
        }

        private CoinFlipMatch
        (
            int lookUpId,
            string roundId,
            string hash,
            string salt,
            string percentage,
            MatchStatus status,
            List<CoinFlipBet> bets,
            DateTime? timerStarted,
            JackpotMatchSetting setting
        )
        {
            LookUpId = lookUpId;
            RoundId = roundId;
            Hash = hash;
            _salt = salt;
            _percentage = percentage;
            Status = status;
            TimerStarted = timerStarted;
            Setting = setting;
            Bets = bets;

            ReadableStatus = status.ToString();
        }
    }

    public class CoinFlipMatchHistory : CoinFlipMatch
    {

        public UserWithQuote Winner { get; }
        public int? WinningTicket { get; }
        
        public CoinFlipMatchHistory(MongoDbModels.JackpotMatch jackpotMatch) : base(jackpotMatch)
        {
            Winner = jackpotMatch.Winner;
            WinningTicket = jackpotMatch.WinningTicket;
        }

    }
}