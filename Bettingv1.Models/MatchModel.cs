using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace Bettingv1.Models
{
    public class MatchModel
    {
        public ObjectId        Id      { get; set; }
        public UserModel       Winner  { get; set; }
        public int             RoundId { get; set; }
        public DateTime        Created { get; set; }
        public decimal         Value   { get; set; }
        public List<ItemModel> Items   { get; set; }
        public List<UserBet>   Bets    { get; set; }

        public MatchModel()
        {
            Bets = new List<UserBet>();
        }
    }
}