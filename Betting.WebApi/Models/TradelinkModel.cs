using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class TradelinkModel
    {
        [Required]
        public string Tradelink { get; set; }
    }
}