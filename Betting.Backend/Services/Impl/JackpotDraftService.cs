using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;

namespace Betting.Backend.Services.Impl
{
    public class JackpotDraftService : DraftService, IJackpotDraftService
    {
        private readonly IRepoServiceFactory              _repoServiceFactory;
        private readonly IRakeService                     _rakeService;
        private readonly ILogService<JackpotDraftService> _logService;


        public JackpotDraftService(IRepoServiceFactory repoServiceFactory, ILogServiceFactory logServiceFactory, IRakeService rakeService)
        {
            _repoServiceFactory = repoServiceFactory;
            _rakeService = rakeService;
            _logService = logServiceFactory.CreateLogger<JackpotDraftService>();
        }

        public async Task ChangeOwnerOfItems(
            List<DatabaseModel.Bet> bets,
            List<DatabaseModel.ItemBetted> itemBets,
            DatabaseModel.User winningUser,
            int matchId,
            decimal rake,
            int gameModeId
        )
        {
            var rakeRes = _rakeService.GetItemsThatWeShouldTake(rake, bets, itemBets, winningUser);

            await TryChangeOwnerOnItemsToWinner(winningUser, rakeRes);
            if (rakeRes.ItemIdsToUs.Count == 0) return;

            var itemsToUs = await _repoServiceFactory.ItemRepoService.FindAsync(rakeRes.ItemIdsToUs);
            var rakeItems = itemsToUs
                .Select(item => new DatabaseModel.RakeItem
                (
                    item.AssetId,
                    item.DescriptionId,
                    item.LocationId,
                    DateTime.Now,
                    matchId,
                    gameModeId
                ))
                .ToList();

            await TryInsertRakeItems(rakeItems);
            await TryDeleteRakeItems(rakeRes);
        }

        public WinningBet GetWinningBet(double percantage, List<DatabaseModel.Bet> bets, List<DatabaseModel.ItemBetted> itemBets)
        {
            var nrOfTickets = (int) (itemBets.Sum(itemBet => itemBet.Value) * 100);
            var winnerTicket = GetWinnigTicket(nrOfTickets, percantage);
            var playersInPool = GetPlayersInPool(bets, itemBets);

            var orederPLayersInPooil = playersInPool.OrderBy(kvp => kvp.Key.Created).ToList();

            var listTicketRanget = GetListTicketRanget(orederPLayersInPooil);

            var winningBet = GetWinningBet(listTicketRanget, winnerTicket);
            return new WinningBet
            {
                Bet = winningBet,
                WinningTicket = winnerTicket
            };
        }


        private async Task TryChangeOwnerOnItemsToWinner(DatabaseModel.User winningUser, RakeService.RakeResult rakeRes)
        {
            try
            {
                await _repoServiceFactory.ItemRepoService.ChangeOwner(rakeRes.ItemIdsToWinner, winningUser);
            }
            catch (SqlException e)
            {
                _logService.Critical(e);
                throw new Exception("THE WINNER DID NOT GET THE ITEMS!", e);
            }
        }

        private async Task TryDeleteRakeItems(RakeService.RakeResult rakeRes)
        {
            try
            {
                await _repoServiceFactory.ItemRepoService.DeleteAsync(rakeRes.ItemIdsToUs);
            }
            catch (SqlException e)
            {
                _logService.Critical(e);
                throw new Exception("WE CAN'T DELET THE RAKE ITEMS WE TOOK!", e);
            }
        }

        private async Task TryInsertRakeItems(List<DatabaseModel.RakeItem> rakeItems)
        {
            try
            {
                await _repoServiceFactory.RakeItemRepoService.InsertAsync(rakeItems);
            }
            catch (SqlException e)
            {
                _logService.Critical(e);
                throw new Exception("WE DID NOT TAKE OUT RAKE!", e);
            }
        }

        private List<TicketRange> GetListTicketRanget(List<KeyValuePair<DatabaseModel.Bet, int>> orederPLayersInPooil)
        {
            var listTicketRanget = new List<TicketRange>();
            int currentStatTicket = 0;
            foreach (var currentPlayer in orederPLayersInPooil)
            {
                var x = new TicketRange
                {
                    Bet = currentPlayer.Key,
                    StartTicket = currentStatTicket,
                    EndTicket = currentStatTicket + currentPlayer.Value
                };
                currentStatTicket += currentPlayer.Value;
                listTicketRanget.Add(x);
            }

            return listTicketRanget;
        }

        private Dictionary<DatabaseModel.Bet, int> GetPlayersInPool(List<DatabaseModel.Bet> betsOnMatch1,
                                                                    List<DatabaseModel.ItemBetted> itemBetsOnMatch1)
        {
            var playersInPool = new Dictionary<DatabaseModel.Bet, int>();

            foreach (var bet in betsOnMatch1)
            {
                playersInPool.Add(bet, 0);
                foreach (var itemBet in itemBetsOnMatch1)
                {
                    if (itemBet.BetId != bet.Id) continue;
                    playersInPool[bet] += (int) (itemBet.Value * 100);
                }
            }

            return playersInPool;
        }

        private DatabaseModel.Bet GetWinningBet(List<TicketRange> ticketRanges, int winningTicket)
        {
            foreach (var ticketRange in ticketRanges)
            {
                if (ticketRange.IsInRange(winningTicket))
                    return ticketRange.Bet;
            }

            throw new NoWinnerFoundException("No winner found! PANIC!");
        }

        public class TicketRange
        {
            public int               StartTicket { get; set; }
            public int               EndTicket   { get; set; }
            public DatabaseModel.Bet Bet         { get; set; }

            public bool IsInRange(int ticketNr)
            {
                return ticketNr >= StartTicket && ticketNr < EndTicket;
            }
        }
    }
}