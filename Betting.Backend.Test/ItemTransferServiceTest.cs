using System.Collections.Generic;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;

namespace Betting.Backend.Test
{
    public class ItemTransferServiceTest
    {
        private readonly IUserRepoService    _fakedUserRepoService;
        private          IRepoServiceFactory _fakedRepoService;
        private          IItemRepoService    _fakedItemRepoService;
        private          IItemService        _fakedItemService;


        public ItemTransferServiceTest()
        {
            _fakedUserRepoService = A.Fake<IUserRepoService>();
            _fakedItemRepoService = A.Fake<IItemRepoService>();

            _fakedItemService = A.Fake<IItemService>();

            _fakedRepoService = A.Fake<IRepoServiceFactory>();
            A.CallTo(() => _fakedRepoService.UserRepoService).Returns(_fakedUserRepoService);
        }

        [Fact]
        public async void TransferItemsSuccess()
        {
            var fromUser = new DatabaseModel.User
            {
                Id = 1
            };
            var toSteamId = "toSteamId";

            var itemsToTransfer = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId3", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId4", DescriptionId = 5},
                new AssetAndDescriptionId {AssetId = "assetId8", DescriptionId = 5},
            };

            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>
            {
                new Item {AssetId = "assetId1", DescriptionId = 1, Id = 1},
                new Item {AssetId = "assetId2", DescriptionId = 1, Id = 2},
                new Item {AssetId = "assetId3", DescriptionId = 1, Id = 3},
                new Item {AssetId = "assetId4", DescriptionId = 5, Id = 4},
                new Item {AssetId = "assetId8", DescriptionId = 5, Id = 5},
            });

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).Returns(new DatabaseModel.User{Id = 1});

            var itemTransferService = new ItemTransferService(_fakedRepoService, _fakedItemService);
            var res = await itemTransferService.TransferItemsAsync(fromUser, toSteamId, itemsToTransfer);

            Assert.True(res);

            A.CallTo(() => _fakedRepoService.ItemRepoService.ChangeOwner(
                A<List<int>>.That.Matches(i => i.Contains(1) && i.Contains(3) && i.Contains(4) && i.Contains(5)),
                A<DatabaseModel.User>.That.Matches(u => u.Id == 1))
            ).MustHaveHappened();
        }


        [Fact]
        public async void TransferItemsFailsDueToItemsIsNotTheOwserts()
        {
            var fromUser = new DatabaseModel.User
            {
                Id = 1
            };

            var toSteamId = "toSteamId";

            var itemsToTransfer = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId3", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId4", DescriptionId = 5},
                new AssetAndDescriptionId {AssetId = "assetId8", DescriptionId = 5},
            };

            var fakedItemService = A.Fake<IItemService>();
            A.CallTo(() => fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>
            {
                new Item {AssetId = "assetId2", DescriptionId = 1, Id = 2},
                new Item {AssetId = "assetId3", DescriptionId = 1, Id = 3},
                new Item {AssetId = "assetId4", DescriptionId = 5, Id = 4},
                new Item {AssetId = "assetId8", DescriptionId = 5, Id = 5},
            });

            var itemTransferService = new ItemTransferService(_fakedRepoService, fakedItemService);
            var res = await itemTransferService.TransferItemsAsync(fromUser, toSteamId, itemsToTransfer);

            Assert.False(res);
            A.CallTo(() => _fakedRepoService.ItemRepoService.ChangeOwner(A<List<int>>._, A<DatabaseModel.User>._)).MustNotHaveHappened();
        }
    }
}