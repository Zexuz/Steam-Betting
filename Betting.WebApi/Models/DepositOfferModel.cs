using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class DepositOfferModel
    {
        [Required]
        public List<ItemModel> Items { get; set; }

        public class ItemModel
        {
            [Required]
            public int AppId { get; set; }

            [Required]
            public string ContextId { get; set; }

            [Required]
            public string AssetId { get; set; }
        }
    }
}