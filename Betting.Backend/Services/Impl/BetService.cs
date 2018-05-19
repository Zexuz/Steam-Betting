using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Helpers;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Backend.Websockets.Models;
using Betting.Models.Models;
using Betting.Repository;
using Betting.Repository.Exceptions;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Services.Interfaces;

namespace Betting.Backend.Services.Impl
{
    public class BetService : IBetService
    {
        private readonly IRepoServiceFactory    _repoServiceFactory;
        private readonly IItemService           _itemService;
        private readonly ITransactionFactory    _transactionFactory;
        private readonly IBetRepoService        _betRepoService;
        private readonly IItemBettedRepoService _itemBettedRepoService;
        private readonly IDiscordService        _discordService;
        private readonly IMatchHubConnections   _matchHub;

        public BetService
        (
            IRepoServiceFactory repoServiceFactory,
            IItemService itemService,
            ITransactionFactory transactionFactoryFactory,
            IBetRepoService betRepoService,
            IItemBettedRepoService itemBettedRepoService,
            IDiscordService discordService,
            IMatchHubConnections matchHub
        )
        {
            _repoServiceFactory = repoServiceFactory;
            _itemService = itemService;
            _transactionFactory = transactionFactoryFactory;
            _betRepoService = betRepoService;
            _itemBettedRepoService = itemBettedRepoService;
            _discordService = discordService;
            _matchHub = matchHub;
        }

        public async Task<List<Item>> GetBettedItemsOnMatch(int matchId, int gameModeId)
        {
            return await GetBettedItemsOnMatchWithMatchId(matchId, gameModeId);
        }

        public async Task PlaceBetOnJackpotMatch
        (
            DatabaseModel.Match match,
            JackpotMatchSetting setting,
            DatabaseModel.GameMode gameMode,
            List<AssetAndDescriptionId> assetAndDescriptionIds,
            string userSteamId
        )
        {
            await PlaceBetOnMatch(match.Id, setting, gameMode, assetAndDescriptionIds, userSteamId);
        }

        public async Task PlaceBetOnCoinFlipMatch
        (
            DatabaseModel.CoinFlip match,
            JackpotMatchSetting setting,
            DatabaseModel.GameMode gameMode,
            List<AssetAndDescriptionId> assetAndDescriptionIds,
            string userSteamId
        )
        {
            await PlaceBetOnMatch(match.Id, setting, gameMode, assetAndDescriptionIds, userSteamId);
        }

        public async Task PlaceBetOnCoinFlipMatch
        (
            DatabaseModel.CoinFlip match,
            JackpotMatchSetting setting,
            DatabaseModel.GameMode gameMode,
            int assetAndDescriptionIdsCount,
            List<DatabaseModel.Item> items,
            DatabaseModel.User user,
            List<DatabaseModel.ItemDescription> uniqueItemDescriptions
        )
        {
            await PlaceBetOnMatch(match.Id, setting, gameMode, assetAndDescriptionIdsCount, items, user, uniqueItemDescriptions);
        }

        public Task<List<DatabaseModel.Bet>> GetBetsFromUser(DatabaseModel.User user)
        {
            return _betRepoService.FindAsync(user);
        }

        public Task<List<DatabaseModel.Bet>> GetBetsFromUser(DatabaseModel.User user, int limit, int? from)
        {
            return _betRepoService.FindAsync(user, limit, from);
        }

        public Task<List<int>> GetBetsForUserOnMatches(DatabaseModel.User user, List<int> matchIds, int gameModeId)
        {
            return _betRepoService.FindAsync(user, matchIds, gameModeId);
        }

        private async Task PlaceBetOnMatch(int matchId, JackpotMatchSetting setting, DatabaseModel.GameMode gameMode,
                                           List<AssetAndDescriptionId> assetAndDescriptionIds, string userSteamId)
        {
            var itemDescs = assetAndDescriptionIds.Select(item => item.DescriptionId).Distinct().ToList();

            var itemsTask = _repoServiceFactory.ItemRepoService.FindAsync(assetAndDescriptionIds);
            var userTask = _repoServiceFactory.UserRepoService.FindAsync(userSteamId);
            var itemDescriptionTask = _repoServiceFactory.ItemDescriptionRepoService.FindAsync(itemDescs);

            await Task.WhenAll(itemsTask, userTask, itemDescriptionTask);

            var items = itemsTask.Result;
            var user = userTask.Result;
            var itemDescriptions = itemDescriptionTask.Result;

            await PlaceBetOnMatch(matchId, setting, gameMode, assetAndDescriptionIds.Count, items, user, itemDescriptions);
        }

        private async Task PlaceBetOnMatch
        (
            int matchId,
            JackpotMatchSetting setting,
            DatabaseModel.GameMode gameMode,
            int assetAndDescriptionIdsCount,
            List<DatabaseModel.Item> items,
            DatabaseModel.User user,
            List<DatabaseModel.ItemDescription> uniqueItemDescriptions
        )
        {
            if (!gameMode.IsEnabled)
                throw new GameModeIsNotEnabledException($"The current gamemode {gameMode.Type} is not enabled");

            if ((setting.AllowCsgo && setting.AllowPubg) == false)
            {
                var oneOrMoreItemsIsPubg = uniqueItemDescriptions.Any(AppIdToGameConverterHelper.IsPubgItem);
                var oneOrMoreItemsIsCsgo = uniqueItemDescriptions.Any(AppIdToGameConverterHelper.IsCsgoItem);

                if (setting.AllowCsgo && oneOrMoreItemsIsPubg)
                    throw new NotAllowedAppIdOnMatchException("You tried to bet a PUBG item on a match that only accepts CSGO");

                if (setting.AllowPubg && oneOrMoreItemsIsCsgo)
                    throw new NotAllowedAppIdOnMatchException("You tried to bet a CSGO item on a match that only accepts PUBG");
            }

            if (uniqueItemDescriptions.Any(description => !description.Valid))
                throw new InvalidItemException("One or more items is not allowed to be used for betting at the moment, refresh the page and try again");

            if (user == null)
                throw new UserDoesNotExistException("Can't find a user");

            if (items.Any(item => item.OwnerId != user.Id))
                throw new UserDoesNotOwnThisItemsException("The user does not own this item!");

            if (items.Count == 0)
                throw new InvalidAssetAndDecriptionIdException("No items with that assetId or descriptionId was found");

            if (items.Count != assetAndDescriptionIdsCount)
                throw new NotSameCountAsExpectedException("assetAndDescriptionIds count != avalibleItems");

            if (items.Count > setting.MaxItemAUserCanBet)
                throw new ToManyItemsOnBetException($"You can max bet {setting.MaxItemAUserCanBet} items, you tried to bet {items.Count}");

            if (items.Count < setting.MinItemAUserCanBet)
                throw new ToFewItemsOnBetException($"You can min bet {setting.MinItemAUserCanBet} items, you tried to bet {items.Count}");

            var sumOfBet = await _itemService.GetSumOfItems(items);
            if (sumOfBet > setting.MaxValueAUserCanBet)
                throw new ToMuchValueOnBetException($"You can max bet {setting.MaxValueAUserCanBet} value, you tried to bet {sumOfBet}");

            if (sumOfBet < setting.MinValueAUserCanBet)
                throw new ToLittleValueOnBetException($"You can min bet {setting.MinValueAUserCanBet} value, you tried to bet {sumOfBet}");


            var avalibleItems = await _itemService.GetAvalibleItemsForUser(user);

            if (avalibleItems.Count == 0)
                throw new NoAvalibleItemsForBettingException("No avalible item for betting");

            if (IsSameItemsInAvalibleItemsAsInItem(items, avalibleItems))
                throw new ItemNotAvalibleException("This item can not be betted now! Is this items already betted on an open match?)");

            await InsertBetWithTransaction(user, matchId, items, gameMode, uniqueItemDescriptions);
            //we have now successfully placed a bet

            if (gameMode.Type == GameModeHelper.GetStringFromType(GameModeType.JackpotCsgo))
                _discordService.JackpotBetAsync(matchId, user.SteamId, sumOfBet);
        }

        private bool IsSameItemsInAvalibleItemsAsInItem(List<DatabaseModel.Item> items, List<Item> avalibleItems)
        {
            //DO NOT USE RESHARPER TO MODIFY THIS CODE!
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in items)
            {
                // ReSharper disable once SimplifyLinqExpression
                if (!avalibleItems.Any(avalibleItem => item.AssetId == avalibleItem.AssetId))
                    return true;
            }

            return false;
        }

        private async Task InsertBetWithTransaction
        (
            DatabaseModel.User user,
            int matchId,
            List<DatabaseModel.Item> items,
            DatabaseModel.GameMode gameMode,
            List<DatabaseModel.ItemDescription> itemDescriptions
        )
        {
            List<DatabaseModel.ItemBetted> itemBets;
            DatabaseModel.Bet insertedBet;
            List<Item> itemModels;
            using (var transaction = _transactionFactory.BeginTransaction())
            {
                try
                {
                    var bet = new DatabaseModel.Bet(user.Id, matchId, gameMode.Id, DateTime.Now);
                    insertedBet = await _betRepoService.InsertAsync(bet, transaction);

                    itemBets = CreateItemBets(insertedBet.Id, itemDescriptions, items, out itemModels);

                    await _itemBettedRepoService.InsertAsync(itemBets, transaction);

                    transaction.Commit();
                }
                catch (SqlException e)
                {
                    throw new InvalidBetException("Can't inset bet due to sql error", e);
                }
            }

            if (gameMode.Type == GameModeHelper.GetStringFromType(GameModeType.JackpotCsgo))
                await SendWebsocketMatchEvent(user, matchId, itemModels, insertedBet);
        }

        private async Task SendWebsocketMatchEvent(DatabaseModel.User user, int matchId, List<Item> itemModels,
                                                   DatabaseModel.Bet insertedBet)
        {
            foreach (var itemModel in itemModels)
            {
                itemModel.Owner = new User
                {
                    ImageUrl = user.ImageUrl,
                    Name = user.Name,
                    SteamId = user.SteamId
                };
            }

            var model = new UserBetsOnMatchModel
            {
                MatchId = matchId,
                Bet = new Bet
                {
                    DateTime = insertedBet.Created,
                    Items = itemModels,
                    User = new User
                    {
                        Name = user.Name,
                        ImageUrl = user.ImageUrl,
                        SteamId = user.SteamId
                    },
                }
            };

            await _matchHub.UserBetsOnMatch(model);
        }

        private List<DatabaseModel.ItemBetted> CreateItemBets
        (
            int betId,
            List<DatabaseModel.ItemDescription> itemDescriptions,
            List<DatabaseModel.Item> items,
            out List<Item> itemModels
        )
        {
            var listItemsBetted = new List<DatabaseModel.ItemBetted>();
            itemModels = new List<Item>();

            foreach (var item in items)
            {
                foreach (var itemDescription in itemDescriptions)
                {
                    if (item.DescriptionId != itemDescription.Id) continue;
                    itemModels.Add(new Item {IconUrl = itemDescription.ImageUrl, Name = itemDescription.Name, Value = itemDescription.Value});
                    listItemsBetted.Add(new DatabaseModel.ItemBetted(betId, itemDescription.Id, item.AssetId, itemDescription.Value));
                    break;
                }
            }


            return listItemsBetted;
        }

        private async Task<List<Item>> GetBettedItemsOnMatchWithMatchId(int matchId, int gameModeId)
        {
            var allBetsOnMatch = await _repoServiceFactory.BetRepoService.FindAsync(matchId, gameModeId);
            var allItemsBettedOnThatMatch = await _repoServiceFactory.ItemBettedRepoService.FindAsync(allBetsOnMatch);
            var allItemDescIds = allItemsBettedOnThatMatch.Select(itemBetted => itemBetted.DescriptionId).ToList();
            var allItemDescs = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync(allItemDescIds);

            var allUserIds = allBetsOnMatch.Select(bet => bet.UserId).ToList();
            var allUsers = await _repoServiceFactory.UserRepoService.FindAsync(allUserIds);

            var usersAssDict = new Dictionary<int, User>();
            foreach (var bet in allBetsOnMatch)
            {
                var databaseUser = allUsers.First(u => u.Id == bet.UserId);
                var user = new User
                {
                    ImageUrl = databaseUser.ImageUrl,
                    Name = databaseUser.Name,
                    SteamId = databaseUser.SteamId
                };
                usersAssDict.Add(bet.Id, user);
            }

            var list = new List<Item>();
            foreach (var itemDescription in allItemDescs)
            {
                foreach (var itemBetted in allItemsBettedOnThatMatch)
                {
                    if (itemBetted.DescriptionId != itemDescription.Id) continue;
                    list.Add(new Item
                    {
                        AssetId = itemBetted.AssetId,
                        DescriptionId = itemBetted.DescriptionId,
                        Id = itemBetted.Id,
                        IconUrl = itemDescription.ImageUrl,
                        Name = itemDescription.Name,
                        Owner = usersAssDict[itemBetted.BetId],
                        Value = itemBetted.Value
                    });
                }
            }

            return list;
        }
    }
}