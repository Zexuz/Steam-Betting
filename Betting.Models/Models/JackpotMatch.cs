using System;
using System.Collections.Generic;
using System.Linq;

namespace Betting.Models.Models
{
    public class JackpotMatch
    {
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

        public List<Bet> Bets { get; }

        public List<Item> ItemsInPool => Bets.SelectMany(bet => bet.Items).ToList();

        public JackpotMatchSetting Setting { get; }


        private readonly string _percentage;
        private readonly string _salt;

        public JackpotMatch
        (
            string roundId,
            string hash,
            string salt,
            string percentage,
            MatchStatus status,
            List<Bet> bets,
            DateTime? timerStarted,
            JackpotMatchSetting setting
        )
        {
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
}