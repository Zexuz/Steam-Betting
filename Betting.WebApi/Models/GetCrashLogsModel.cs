using System;

namespace Betting.WebApi.Models
{
    public class GetCrashLogsModel
    {
        public DateTime Start   { get; set; }
        public DateTime End     { get; set; }
        public string   SteamId { get; set; }
    }
}