using System;

namespace Betting.Models.Models
{
    public class UserDetailed : User
    {
        public DateTime Created    { get; set; }
        public DateTime LastActive { get; set; }
        public string   Tradelink  { get; set; }
        public string   Quote      { get; set; }
    }
}