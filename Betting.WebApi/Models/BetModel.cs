using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class BetModel
    {
        [Required]
        public List<AssetAndDescriptionModel> Items { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int RoundId { get; set; }

        public class AssetAndDescriptionModel
        {
            [Required]
            public string AssetId { get; set; }

            [Required]
            public int DescriptionId { get; set; }
        }
    }
}