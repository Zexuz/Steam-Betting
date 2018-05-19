using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository;
using Betting.Repository.Extensions;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Services.Interfaces;

namespace Betting.Backend.Services.Impl
{
    public class ItemService : IItemService
    {
        private readonly IMatchRepoService                  _matchRepoService;
        private readonly IBetRepoService                    _betRepoService;
        private readonly IItemRepoService                   _itemRepoSerive;
        private readonly IItemDescriptionRepoService        _itemDescriptionRepoService;
        private readonly IItemBettedRepoService             _itemBettedRepoService;
        private readonly IOfferTranascrionRepoService       _offerTransactionRepoService;
        private readonly IItemInOfferTransactionRepoService _itemsInOfferTransactionRepoService;
        private readonly ICoinFlipMatchRepoService          _coinFlipMatchRepoService;
        private readonly IGameModeRepoService               _gameModeRepoService;

        public ItemService(IRepoServiceFactory repoServiceFactory)
        {
            _matchRepoService = repoServiceFactory.MatchRepoService;
            _betRepoService = repoServiceFactory.BetRepoService;
            _itemRepoSerive = repoServiceFactory.ItemRepoService;
            _itemDescriptionRepoService = repoServiceFactory.ItemDescriptionRepoService;
            _itemBettedRepoService = repoServiceFactory.ItemBettedRepoService;
            _itemsInOfferTransactionRepoService = repoServiceFactory.ItemInOfferTransactionRepoService;
            _offerTransactionRepoService = repoServiceFactory.OfferTranascrionRepoService;
            _coinFlipMatchRepoService = repoServiceFactory.CoinFlipMatchRepoService;
            _gameModeRepoService = repoServiceFactory.GameModeRepoService;
        }


        public async Task<List<Item>> GetAvalibleItemsForUser(DatabaseModel.User user)
        {
            var jackpotGameModeTask = _gameModeRepoService.Find(GameModeHelper.GetStringFromType(GameModeType.JackpotCsgo));
            var coinFlipGameModeTask = _gameModeRepoService.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip));

            var currentMatchTask = _matchRepoService.GetCurrentMatch();
            var notClosedCoinFlipmatchesTask = _coinFlipMatchRepoService.FindAllNotClosedMatches();

            await Task.WhenAll(jackpotGameModeTask, coinFlipGameModeTask, currentMatchTask, notClosedCoinFlipmatchesTask);

            var coinFlipGameMode = coinFlipGameModeTask.Result;
            var currentMatch = currentMatchTask.Result;
            var jackpotGameMode = jackpotGameModeTask.Result;
            var notClosedCoinFlipmatches = notClosedCoinFlipmatchesTask.Result;
            
            var coinflipLookUpGameModeBet = new LookUpGameModeBet
            {
                GameMode = coinFlipGameMode,
                User = user,
                MatchIds = notClosedCoinFlipmatches.Select(coin => coin.Id).ToList()
            };

            var jackpotLookUpGameModeBet = new LookUpGameModeBet
            {
                GameMode = jackpotGameMode,
                User = user,
                MatchIds = new List<int> {currentMatch.Id}
            };

            var userBets = await _betRepoService.FindAsync(new List<LookUpGameModeBet>
            {
                coinflipLookUpGameModeBet,
                jackpotLookUpGameModeBet
            });

            var offers = await _offerTransactionRepoService.FindActiveAsync(user);

            
            var itemThatUserOwns = await GetItemsThatUsersOwns(user);

            if (userBets.Count == 0 && offers.Count == 0)
                return itemThatUserOwns;
            
            
            if (userBets.Count > 0 && offers.Count == 0)
                return await GetNoneBetedItems(userBets, itemThatUserOwns);
            
            
            if (userBets.Count == 0 && offers.Count > 0)
                return await GetItemsThatIsNotInAOffer(offers, itemThatUserOwns);
            
            
            var itemNotInOffer            = await GetItemsThatIsNotInAOffer(offers, itemThatUserOwns);
            var itemNotInBetAndNotInOffer = await GetNoneBetedItems(userBets, itemNotInOffer);
            return itemNotInBetAndNotInOffer;
        }

        public async Task<decimal> GetSumOfItems(List<DatabaseModel.Item> items)
        {
            var dict = await _itemDescriptionRepoService.ValueOfItemDescriptions(items.Select(i => i.DescriptionId).ToList());

            var sum = new decimal(0);
            foreach (var kvp in dict)
            {
                sum += items.Where(item => kvp.Key == item.DescriptionId).Sum(item => kvp.Value);
            }

            return sum;
        }

        private async Task<List<Item>> GetItemsThatIsNotInAOffer(List<DatabaseModel.OfferTransaction> offers, List<Item> itemsThatUserOwns)
        {
            var itemInOfferTransactions = await _itemsInOfferTransactionRepoService.FindAsync(offers);

            return itemsThatUserOwns.Where(item => itemInOfferTransactions.All(iOffer => iOffer.AssetId != item.AssetId)).ToList();
        }

        private async Task<List<Item>> GetItemsThatUsersOwns(DatabaseModel.User user)
        {
            var rawItems = await _itemRepoSerive.FindAsync(user);

            var list = rawItems.Select(item => item.DescriptionId).ToList();
            var itemDescriptions = await _itemDescriptionRepoService.FindAsync(list);

            return rawItems.MapItemsToItemsDescription(itemDescriptions);
        }

        private async Task<List<Item>> GetNoneBetedItems(List<DatabaseModel.Bet> betOnThisMatch, List<Item> itemsThatUserOwns)
        {
            var itemBets = await _itemBettedRepoService.FindAsync(betOnThisMatch);

            var returnItems = new List<Item>();
            foreach (var item in itemsThatUserOwns)
            {
                var foundIt = false;
                foreach (var itemBetted in itemBets)
                {
                    if (item.AssetId == itemBetted.AssetId && item.DescriptionId == itemBetted.DescriptionId)
                        foundIt = true;
                }

                if (!foundIt)
                    returnItems.Add(item);
            }

            return returnItems;
        }
    }
}