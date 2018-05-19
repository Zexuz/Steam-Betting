using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Betting.Models.Models;

namespace Betting.WebApi.Models
{
    public class CreateCoinFlipModel
    {
        [Required]
        public List<AssetAndDescriptionId> Items { get; set; }

        [Required]
        public CoinFlipSettingModel Settings { get; set; }

        [Required]
        public string CoinFlipPreHash { get; set; } 

        [Required]
        public bool IsHead { get; set; }
    }
}