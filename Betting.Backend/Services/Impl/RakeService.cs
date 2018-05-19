using System.Collections.Generic;
using System.Linq;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;

namespace Betting.Backend.Services.Impl
{
    public class RakeService : IRakeService
    {
        public RakeResult GetItemsThatWeShouldTake
        (
            decimal rake,
            List<DatabaseModel.Bet> bets,
            List<DatabaseModel.ItemBetted> bettedITems,
            DatabaseModel.User winner
        )
        {
            var result = new RakeResult
            {
                ItemIdsToWinner = new List<AssetAndDescriptionId>(),
                ItemIdsToUs     = new List<AssetAndDescriptionId>()
            };

            var winningBet            = bets.Single(bet => bet.UserId == winner.Id);
            var allItemsExceptWinners = bettedITems
                .Where(iBet => iBet.BetId != winningBet.Id)
                .OrderByDescending(iBet => iBet.Value)
                .ToList();

            var totalSum          = bettedITems.Sum(i => i.Value);
            var maxValueWeCanTake = totalSum * (rake / 100);
            var lowestItemValue   = allItemsExceptWinners.Last().Value;
            if (rake == 0 || allItemsExceptWinners.Count < 2 || lowestItemValue > maxValueWeCanTake || PromotionInName(winner))
            {
                result.ItemIdsToWinner                              = allItemsExceptWinners
                    .Select(i => new AssetAndDescriptionId {AssetId = i.AssetId, DescriptionId = i.DescriptionId})
                    .ToList();
                return result;
            }

            GiveHighestValuedItemToWinner(allItemsExceptWinners, result);
            TakeRake(maxValueWeCanTake, allItemsExceptWinners, result);
            GiveRestToWinner(allItemsExceptWinners, result);

            return result;
        }

        private bool PromotionInName(DatabaseModel.User winner)
        {
            var name = winner.Name.ToLower();

            return name.Contains("DomainName.com") || name.Contains("DomainName");
        }

        private static void GiveRestToWinner(List<DatabaseModel.ItemBetted> allItemsExceptWinners, RakeResult result)
        {
            foreach (var item in allItemsExceptWinners)
            {
                result.ItemIdsToWinner.Add(new AssetAndDescriptionId {AssetId = item.AssetId, DescriptionId = item.DescriptionId});
            }
        }

        private static void TakeRake(decimal maxValueWeCanTake, List<DatabaseModel.ItemBetted> allItemsExceptWinners, RakeResult result)
        {
            decimal sumWeTake = 0;

            var list = new List<AssetAndDescriptionId>();
            foreach (var item in allItemsExceptWinners)
            {
                if (item.Value + sumWeTake > maxValueWeCanTake) continue;
                sumWeTake                                                 += item.Value;
                result.ItemIdsToUs.Add(new AssetAndDescriptionId {AssetId = item.AssetId, DescriptionId = item.DescriptionId});
                list.Add(new AssetAndDescriptionId {AssetId               = item.AssetId, DescriptionId = item.DescriptionId});
            }

            for (int i = allItemsExceptWinners.Count - 1; i >= 0; i--)
            {
                foreach (var assetAndDescriptionId in list)
                {
                    if (allItemsExceptWinners[i].AssetId       != assetAndDescriptionId.AssetId ||
                        allItemsExceptWinners[i].DescriptionId != assetAndDescriptionId.DescriptionId) continue;
                    allItemsExceptWinners.RemoveAt(i);
                    break;
                }
            }
        }

        private static void GiveHighestValuedItemToWinner(List<DatabaseModel.ItemBetted> allItemsExceptWinners, RakeResult result)
        {
            var lastItemId = allItemsExceptWinners.First();
            allItemsExceptWinners.RemoveAt(0);
            result.ItemIdsToWinner.Add(new AssetAndDescriptionId {AssetId = lastItemId.AssetId, DescriptionId = lastItemId.DescriptionId});
        }

        public class RakeResult
        {
            public List<AssetAndDescriptionId> ItemIdsToWinner { get; set; }
            public List<AssetAndDescriptionId> ItemIdsToUs     { get; set; }
        }
    }
}