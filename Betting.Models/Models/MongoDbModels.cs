using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Betting.Models.Models
{
    public class MongoDbModels
    {
        public class JackpotMatch
        {
            public int LookUpId { get; set; }

            [BsonId]
            public string RoundId { get; set; }

            public string Hash       { get; set; }
            public string Salt       { get; set; }
            public string Percentage { get; set; }
            public int    Status     { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime? TimerStarted { get; set; }

            public UserWithQuote Winner        { get; set; }
            public int?          WinningTicket { get; set; }

            public List<CoinFlipBet>   Bets    { get; set; }
            public JackpotMatchSetting Setting { get; set; }
        }

        public class PreHash
        {
            [BsonId]
            public string Hash { get; set; }

            public string Salt       { get; set; }
            public string Percentage { get; set; }

            public string UserSteamId { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime Created { get; set; }
        }
    }
}