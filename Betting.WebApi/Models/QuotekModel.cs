using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class QuotekModel
    {
        [Required]
        public string Quote { get; set; }
    }
}