using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class WithdrawOfferModel
    {
        [Required]
        public List<AssetAndDescriptionModel> Items { get; set; }

        public class AssetAndDescriptionModel
        {
            [Required]
            public string AssetId { get; set; }

            [Required]
            public int DescriptionId { get; set; }
        }
    }
}