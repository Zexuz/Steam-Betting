using System.Collections.Generic;

namespace Betting.Models.Models
{
    
    public class LookUpGameModeBet
    {
        public DatabaseModel.GameMode       GameMode { get; set; }
        public List<int> MatchIds   { get; set; }
        public DatabaseModel.User User { get; set; }
    }
}