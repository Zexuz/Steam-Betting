namespace Betting.Models.Models
{
    public class Ticket
    {
        public string Message  { get; set; }
        public string SteamId  { get; set; }
        public string Title    { get; set; }
        public string TicketId { get; set; }
    }

    public class TicketSupportAdmin : Ticket
    {
        public string ToSteamId { get; set; }
        public string Name      { get; set; }
    }
}