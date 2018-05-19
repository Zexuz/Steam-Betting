using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class SendChatMessageModel
    {
        [Required]
        public string Message { get; set; }
    }
}