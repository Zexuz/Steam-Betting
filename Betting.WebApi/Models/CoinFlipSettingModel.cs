using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class CoinFlipSettingModel
    {
        [Required]
        public bool AllowCsgo { get; set; }

        [Required]
        public bool AllowPubg { get; set; }

//        [Required]
//        public int MaxItem { get; set; }
//
//        [Required]
//        public int MinItem { get; set; }

        [Required]
        public int Diff { get; set; }
    }
}