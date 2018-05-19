using Betting.Models.Models;

namespace Betting.WebApi.Models
{
    public class BotInfo
    {
        public int               ItemsInBot     => RakeItemsCount + UserItemCount;
        public int               RakeItemsCount { get; set; }
        public int               UserItemCount  { get; set; }
        public DatabaseModel.Bot Bot            { get; set; }
    }
}