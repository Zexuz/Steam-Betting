using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;

namespace Betting.Backend.Services.Impl
{
    public class UserService : IUserService
    {
        private readonly IRepoServiceFactory _repoServiceFactory;
        private readonly IBetService         _betService;
        private readonly IDiscordService     _discordService;

        public UserService(IRepoServiceFactory repoServiceFactory, IBetService betService, IDiscordService discordService)
        {
            _repoServiceFactory = repoServiceFactory;
            _betService = betService;
            _discordService = discordService;
        }


        public async Task<bool> UserLoggedIn(DatabaseModel.User userToLookup)
        {
            var userRepoService = _repoServiceFactory.UserRepoService;
            var user = await userRepoService.FindAsync(userToLookup.SteamId);
            if (user != null)
            {
                return await UpdateUserInfoIfNeeded(userToLookup, user);
            }

            await userRepoService.InsertAsync(userToLookup);

            try
            {
                _discordService.AddUserAsync(userToLookup.SteamId, userToLookup.Name);
            }
            catch (Exception)
            {
                // ignored
            }

            return true;
        }

        public async Task<bool> UpdateUserInfoIfNeeded(DatabaseModel.User newUserInfo, DatabaseModel.User user)
        {
            var userRepoService = _repoServiceFactory.UserRepoService;
            if (user.ImageUrl != newUserInfo.ImageUrl && user.Name != newUserInfo.Name)
            {
                await userRepoService.UpdateImageAndNameAsync(user.SteamId, newUserInfo.Name, newUserInfo.ImageUrl);
                return false;
            }

            if (user.ImageUrl != newUserInfo.ImageUrl)
                await userRepoService.UpdateImageAsync(user.SteamId, newUserInfo.ImageUrl);
            if (user.Name != newUserInfo.Name)
                await userRepoService.UpdateNameAsync(user.SteamId, newUserInfo.Name);
            return false;
        }

        public async Task<DatabaseModel.User> FindAsync(string steamId)
        {
            return await _repoServiceFactory.UserRepoService.FindAsync(steamId);
        }


        public async Task<List<MatchHistory>> GetMatchesUserWon(DatabaseModel.User user, int nrOfItems, int? from)
        {
            var matchesWon = await _repoServiceFactory.MatchRepoService.GetMatchesUserWon(user, nrOfItems, from);
            var matchHistory = await GetMatchHistories(user, matchesWon);

            return matchHistory;
        }

        private async Task<List<MatchHistory>> GetMatchHistories(DatabaseModel.User user, List<DatabaseModel.Match> matches)
        {
            var matchHistory = new List<MatchHistory>();

            foreach (var match in matches)
            {
                decimal matchValue = 0;
                decimal userValue = 0;
                var itemsInPot = 0;

                var bettedItemsOnMatch = await _betService.GetBettedItemsOnMatch(match.Id, match.GameModeId);

                itemsInPot = bettedItemsOnMatch.Count;

                if (!match.WinnerId.HasValue)
                    throw new Exception("Something is badly wrong! No winnerid on closed match!!!!");

                var matchWinner = await _repoServiceFactory.UserRepoService.FindAsync(match.WinnerId.Value);
                foreach (var item in bettedItemsOnMatch)
                {
                    matchValue += item.Value;
                    if (item.Owner.SteamId == user.SteamId)
                        userValue += item.Value;
                }


                matchHistory.Add(new MatchHistory
                {
                    Created = match.Created,
                    Hash = match.Hash,
                    Percentage = match.Percentage,
                    RoundId = match.RoundId,
                    Salt = match.Salt,
                    MatchValue = matchValue,
                    UserValue = userValue,
                    ItemsInMatch = itemsInPot,
                    WinnerSteamId = matchWinner.SteamId
                });
            }

            return matchHistory;


//
//            var winnerSteamIds = new List<int>();
//            foreach (var match in matches)
//            {
//                if (!match.WinnerId.HasValue) ;
//                winnerSteamIds.Add(match.WinnerId.Value);
//            }
//            var winners = await _repoServiceFactory.UserRepoService.FindAsync(winnerSteamIds);
//
//
//            if (matches.Count != bets.Count)
//                throw new Exception($"Matches won does not equal bets somehow?{matches.Count}-{bets.Count}"); //This should never happen!
//
//            for (var i = 0; i < matches.Count; i++)
//            {
//                var match = matches[i];
//                var betId = bets[i];
//                decimal value = 0;
//
//                int nrOfItemsBetted = 0;
//                foreach (var itemBetted in itemsBetted)
//                {
//                    if (betId != itemBetted.BetId) continue;
//                    value += itemBetted.Value;
//                    nrOfItemsBetted++;
//                }
//
//                matchHistory.Add(new MatchHistory
//                {
//                    Created = match.Created,
//                    Hash = match.Hash,
//                    Percentage = match.Percentage,
//                    RoundId = match.RoundId,
//                    Salt = match.Salt,
//                    MatchValue = value,
//                    NrOfItemsBetted = nrOfItemsBetted,
//                    WinnerSteamId = winners.First(u=> match.WinnerId != null && u.Id == match.WinnerId.Value).SteamId
//                });
//            }
//
//            return matchHistory;
        }

        public async Task<WinsAndLosses> GetMatchHistoryForUser(DatabaseModel.User user, int limit, int? from)
        {
            var userBets = await _betService.GetBetsFromUser(user, limit, from);
            var matchIds = userBets.Select(bet => bet.MatchId).ToList();
            var matches = await _repoServiceFactory.MatchRepoService.FindByMatchIdsAsync(matchIds);

            var res = new WinsAndLosses
            {
                Loses = new List<MatchHistory>(),
                Wins = new List<MatchHistory>(),
            };

            var matchHistory = await GetMatchHistories(user, matches);

            foreach (var match in matchHistory)
            {
                if (match.WinnerSteamId == user.SteamId)
                    res.Wins.Add(match);
                else
                    res.Loses.Add(match);
            }

            return res;
        }

//        public async Task<List<MatchHistory>> GetMatchesUserLost(DatabaseModel.User user, int nrOfItems, int? from)
//        {
//            var userbets = await _betService.GetBetsFromUser(user, nrOfItems, from);
//
//            var idWhereUserWon = await _repoServiceFactory.MatchRepoService.GetMatchIdsUserWon(user);
//
//            var matchIdsNotWon = userbets.Where(bet => !idWhereUserWon.Contains(bet.MatchId)).Select(bet => bet.MatchId).ToList();
//
//            var matches = await _repoServiceFactory.MatchRepoService.FindAsync(matchIdsNotWon);
//            
//            if (matches.Any(m => m.Status == MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open)))
//                throw new Exception("Returning sencetive match data on a open match!");
//            
//            var matchHistory = await GetMatchHistories(user, matchIdsNotWon, matches);
//         
//            return matchHistory;
//        }
    }


    public struct WinsAndLosses
    {
        public List<MatchHistory> Wins  { get; set; }
        public List<MatchHistory> Loses { get; set; }
    }
}