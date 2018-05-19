using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class TicketCreateModel
    {
        [Required]
        public string Message { get; set; }

        [Required]
        public string Title { get; set; }

        public string Name { get; set; }
    }

    public class TicketResponseModel
    {
        [Required]
        public string Message { get; set; }
    }


    public class SupportResponseModel : TicketResponseModel
    {
        [Required]
        public string Name { get; set; }
    }

    public class SupportCreateModel : TicketCreateModel
    {
        [Required]
        public string SteamId { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class SupportChangeStatus
    {
        [Required]
        public string Status { get; set; }
    }


    public class SupportListTicket
    {
        public string Status   { get; set; }
        public string SteamId  { get; set; }
        public string TicketId { get; set; }
    }
}