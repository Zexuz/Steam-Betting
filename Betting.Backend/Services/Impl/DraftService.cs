using System;
using Betting.Models.Models;

namespace Betting.Backend.Services.Impl
{
    public abstract class DraftService
    {
        protected int GetWinnigTicket(int totalTickets, double winPercentage)
        {
            return (int) Math.Floor((totalTickets - 0.00000000000001) * (winPercentage / 100));
        }
    }

    public class WinningBet
    {
        public DatabaseModel.Bet Bet           { get; set; }
        public int               WinningTicket { get; set; }
    }
}