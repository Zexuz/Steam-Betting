using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Extension;
using Betting.Backend.Services.Interfaces;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Services.Interfaces;
using Bet = Betting.Models.Models.Bet;
using Item = Betting.Models.Models.Item;

namespace Betting.Backend.Services.Impl
{
    public abstract class JackpotMatchService
    {
        private readonly IMatchRepoService   _matchRepoService;
        private readonly IRepoServiceFactory _repoServiceFactory;
        private readonly IBetService         _betService;
        private readonly IHashService        _hashService;
        private readonly IRandomService      _randomService;

        protected JackpotMatchService
        (
            IRepoServiceFactory repoServiceFactory,
            IBetService betService,
            IHashService hashService,
            IRandomService randomService
        )
        {
            _matchRepoService = repoServiceFactory.MatchRepoService;
            _repoServiceFactory = repoServiceFactory;
            _betService = betService;
            _hashService = hashService;
            _randomService = randomService;
        }

        protected async Task<DatabaseModel.Match> CreateNewMatchAsync(int roundId, DatabaseModel.GameMode currentGameMode)
        {
            var percentage = _randomService.GeneratePercentage();
            var salt = _randomService.GenerateSalt();
            var hash = _hashService.CreateBase64Sha512Hash(percentage, salt);
            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);

            var match = new DatabaseModel.Match
            (
                roundId,
                salt,
                hash,
                percentage,
                status,
                null,
                null,
                currentGameMode.CurrentSettingId,
                currentGameMode.Id,
                DateTime.Now
            );
            return await _matchRepoService.InsertAsync(match);
        }

        protected async Task<JackpotMatch> GetMatchAsync(int id, DatabaseModel.JackpotSetting settings, DatabaseModel.GameMode gameMode)
        {
            var match = await _matchRepoService.FindAsync(id);
            return await GetMatch(settings, match,gameMode);
        }

        protected async Task<JackpotMatch> GetCurrentMatch(DatabaseModel.JackpotSetting settings,DatabaseModel.GameMode gameMode)
        {
            var match = await _matchRepoService.GetCurrentMatch();
            return await GetMatch(settings, match,gameMode);
        }

        protected async Task<List<JackpotMatchHistoryBasic>> GetMatchHistory(int fromId, int i)
        {
            var matches = await _matchRepoService.FindRangeAsync(fromId, i);

            var list = new List<JackpotMatchHistoryBasic>();
            foreach (var match in matches)
            {
                list.Add(await GetHistoryBasic(match));
            }

            return list;
        }

        protected async Task<JackpotMatchHistoryBasic> GetHistoryBasic(DatabaseModel.Match match)
        {
            if (!match.WinnerId.HasValue)
                throw new Exception("We have no winner for this match");

            var winningUser = await _repoServiceFactory.UserRepoService.FindAsync(match.WinnerId.Value);

            var matchHistory =
                new JackpotMatchHistoryBasic
                {
                    Created = match.Created,
                    Hash = match.Hash,
                    Percentage = match.Percentage,
                    RoundId = match.RoundId,
                    Salt = match.Salt,
                    UserWinnerImgUrl = winningUser.ImageUrl,
                    UserWinnerName = winningUser.Name,
                    UserWinnerQuote = winningUser.Quote,
                    UserWinnerSteamId = winningUser.SteamId
                };

            return matchHistory;
        }

        protected async Task<List<Bet>> GetBets(List<Item> items, int matchId, DatabaseModel.GameMode gameMode)
        {
            var uniqueUsers = UniqueUsers(items);

            var databaseBets = await _repoServiceFactory.BetRepoService.FindAsync(matchId, gameMode.Id);
            var databaseUsers = await _repoServiceFactory.UserRepoService.FindAsync(uniqueUsers.Select(u => u.SteamId).ToList());

            var bets = uniqueUsers.Select(user => new Bet
                {
                    Items = items.Where(item => item.Owner.SteamId == user.SteamId).ToList(),
                    User = user,
                })
                .ToList();

            foreach (var databaseUser in databaseUsers)
            {
                foreach (var databaseBet in databaseBets)
                {
                    if (databaseBet.UserId != databaseUser.Id) continue;
                    foreach (var bet in bets)
                    {
                        if (bet.User.SteamId != databaseUser.SteamId) continue;

                        bet.DateTime = databaseBet.Created;
                    }
                }
            }

            bets = bets.OrderBy(o => o.DateTime).ToList();
            decimal totalValue = 0;
            foreach (var bet in bets)
            {
                var betValue = bet.Items.Sum(a => a.Value);


                var ticket = new RoundTicket();
                ticket.Start = ((int) (totalValue * 100)) + 1;
                ticket.End = (int) ((betValue             + totalValue) * 100);

                bet.Tickets = ticket;
                totalValue += betValue;
            }

            return bets;
        }

        private async Task<JackpotMatch> GetMatch(DatabaseModel.JackpotSetting settings, DatabaseModel.Match match, DatabaseModel.GameMode gameMode)
        {
            var items = await _betService.GetBettedItemsOnMatch(match.Id,gameMode.Id) ?? new List<Item>();

            return new JackpotMatch(
                match.RoundId.ToString(),
                match.Hash,
                match.Salt,
                match.Percentage,
                MatchStatusHelper.GetMatchStatusFromInt(match.Status),
                await GetBets(items, match.Id,gameMode),
                match.TimerStarted,
                settings.ToJackpotMatchSetting()
            );
        }

        private List<User> UniqueUsers(List<Item> items)
        {
            var uniqueUsers = new List<User>();
            foreach (var item in items)
            {
                if (uniqueUsers.All(user => user.SteamId != item.Owner.SteamId))
                    uniqueUsers.Add(item.Owner);
            }

            return uniqueUsers;
        }
    }
}