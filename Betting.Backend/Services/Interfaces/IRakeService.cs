using System.Collections.Generic;
using Betting.Backend.Services.Impl;
using Betting.Models.Models;

namespace Betting.Backend.Services.Interfaces
{
    public interface IRakeService
    {
        RakeService.RakeResult GetItemsThatWeShouldTake
        (
            decimal rake,
            List<DatabaseModel.Bet> bets,
            List<DatabaseModel.ItemBetted> bettedITems,
            DatabaseModel.User winner
        );
    }
}