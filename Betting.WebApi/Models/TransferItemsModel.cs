using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Betting.Models.Models;

namespace Betting.WebApi.Models
{
    public class TransferItemsModel
    {
        [Required]
        public string ToSteamId { get; set; }

        [Required]
        public List<AssetAndDescriptionId> Items { get; set; }
    }
}