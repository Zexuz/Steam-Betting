using System.Collections.Generic;
using System.Linq;

namespace Betting.WebApi.Models
{
    public class BotInfoResponseModel
    {
        public int           MaxCapacity            => BotInfos.Count * 1000;
        public int           TotalItemsInSystem     => BotInfos.Sum(bot => bot.ItemsInBot);
        public int           TotalUserItemsInSystem => BotInfos.Sum(bot => bot.UserItemCount);
        public int           TotalRakeItemsInSystem => BotInfos.Sum(bot => bot.RakeItemsCount);
        public List<BotInfo> BotInfos               { get; set; }
    }
}