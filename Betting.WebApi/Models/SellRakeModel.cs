using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class SellRakeModel
    {
        [Required]
        public int AppId { get; set; }

        [Required]
        public string ContextId { get; set; }

    }
}