using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;

namespace Betting.Backend.Services.Impl
{
    public class ItemTransferService : IItemTransferService
    {
        private readonly IRepoServiceFactory _repoServiceFactory;
        private readonly IItemService _itemService;

        public ItemTransferService(IRepoServiceFactory repoServiceFactory, IItemService itemService)
        {
            _repoServiceFactory = repoServiceFactory;
            _itemService = itemService;
        }
        
        
        public async Task<bool> TransferItemsAsync(DatabaseModel.User fromUser, string toSteamId, List<AssetAndDescriptionId> items)
        {
            if (items == null)
                throw new ArgumentNullException();

            if (fromUser == null)
                throw new ArgumentNullException();

            if (items.Count == 0)
                return false;

            var userToReceiveItem = await _repoServiceFactory.UserRepoService.FindAsync(toSteamId);
            if (userToReceiveItem == null)
                return false;

            var avalibeItems = await _itemService.GetAvalibleItemsForUser(fromUser);

            var ids = new List<int>();
            foreach (var assetAndDescriptionId in items)
            {
                var didFind = false;

                foreach (var avalibeItem in avalibeItems)
                {
                    if (avalibeItem.AssetId != assetAndDescriptionId.AssetId || avalibeItem.DescriptionId != assetAndDescriptionId.DescriptionId)
                        continue;

                    didFind = true;
                    ids.Add(avalibeItem.Id);
                    break;
                }

                if (!didFind)
                    return false; //one (or more) is not avalibe for some reason. (Not the owner. Already betted etc.)
            }

            await _repoServiceFactory.ItemRepoService.ChangeOwner(ids, userToReceiveItem);
            return true;
        }
    }
}