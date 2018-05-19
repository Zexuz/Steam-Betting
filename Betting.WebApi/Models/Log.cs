using System;
using System.Collections.Generic;

namespace Betting.WebApi.Models
{
    public class Log
    {
        public string         SteamId { get; set; }
        public List<LogIndex> Logs    { get; set; }
    }

    public class LogIndex
    {
        public DateTime Time   { get; set; }
        public string   Value  { get; set; }
        public string   Action { get; set; }
    }
}