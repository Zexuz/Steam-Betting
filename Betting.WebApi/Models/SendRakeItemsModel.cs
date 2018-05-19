using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class SendRakeItemsModel
    {
        [Required]
        public List<SendRakeItem> Items { get; set; }
    }

    public class SendRakeItem
    {
        [Required]
        public string AssetId { get; set; }

        [Required]
        public string ContextId { get; set; }

        [Required]
        public int AppId { get; set; }

        [Required]
        public string InBotSteamId { get; set; }
    }
}