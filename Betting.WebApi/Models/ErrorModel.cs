using System;

namespace Betting.WebApi.Models
{
    public class ErrorModel
    {
        public string   ErrorPath  { get; set; }
        public string   Message    { get; set; }
        public DateTime Time       { get; set; }
        public string   StackTrace { get; set; }
        public string   SteamId    { get; set; }
    }
}