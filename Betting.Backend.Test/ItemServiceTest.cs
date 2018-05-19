using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Betting.Backend.Services.Impl;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;

namespace Betting.Backend.Test
{
    public class ItemServiceTest
    {
        private readonly IMatchRepoService                  _matchRepoService;
        private readonly IBetRepoService                    _betRepoService;
        private readonly IItemRepoService                   _itemRepoService;
        private readonly IItemDescriptionRepoService        _descriptionRepoService;
        private readonly IItemBettedRepoService             _itemBettedRepoSerivce;
        private          IRepoServiceFactory                _fakedRepoService;
        private          IOfferTranascrionRepoService       _offerTransactionRepoService;
        private          IItemInOfferTransactionRepoService _itemsInOfferTransactionRepoService;
        private readonly ICoinFlipMatchRepoService          _fakedCoinFlipService;
        private readonly IUserRepoService _fakedUserRepoService;

        public ItemServiceTest()
        {
            _matchRepoService = A.Fake<IMatchRepoService>();
            _betRepoService = A.Fake<IBetRepoService>();
            _itemRepoService = A.Fake<IItemRepoService>();
            _descriptionRepoService = A.Fake<IItemDescriptionRepoService>();
            _itemBettedRepoSerivce = A.Fake<IItemBettedRepoService>();
            _offerTransactionRepoService = A.Fake<IOfferTranascrionRepoService>();
            _itemsInOfferTransactionRepoService = A.Fake<IItemInOfferTransactionRepoService>();
            _fakedCoinFlipService = A.Fake<ICoinFlipMatchRepoService>();
            _fakedUserRepoService = A.Fake<IUserRepoService>();


            _fakedRepoService = A.Fake<IRepoServiceFactory>();
            A.CallTo(() => _fakedRepoService.MatchRepoService).Returns(_matchRepoService);
            A.CallTo(() => _fakedRepoService.BetRepoService).Returns(_betRepoService);
            A.CallTo(() => _fakedRepoService.ItemRepoService).Returns(_itemRepoService);
            A.CallTo(() => _fakedRepoService.ItemDescriptionRepoService).Returns(_descriptionRepoService);
            A.CallTo(() => _fakedRepoService.ItemBettedRepoService).Returns(_itemBettedRepoSerivce);
            A.CallTo(() => _fakedRepoService.OfferTranascrionRepoService).Returns(_offerTransactionRepoService);
            A.CallTo(() => _fakedRepoService.ItemInOfferTransactionRepoService).Returns(_itemsInOfferTransactionRepoService);
            A.CallTo(() => _fakedRepoService.CoinFlipMatchRepoService).Returns(_fakedCoinFlipService);
            A.CallTo(() => _fakedRepoService.UserRepoService).Returns(_fakedUserRepoService);

        }
//
//        [Fact]
//        public async void TransferItemsSuccess()
//        {
//            var fromUser = new DatabaseModel.User
//            {
//                Id = 1
//            };
//            var toSteamId = "toSteamId";
//
//            var itemsToTransfer = new List<AssetAndDescriptionId>
//            {
//                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1},
//                new AssetAndDescriptionId {AssetId = "assetId3", DescriptionId = 1},
//                new AssetAndDescriptionId {AssetId = "assetId4", DescriptionId = 5},
//                new AssetAndDescriptionId {AssetId = "assetId8", DescriptionId = 5},
//            };
//
//            var fakedItemService = A.Fake<IItemService>();
//            A.CallTo(() => fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>
//            {
//                new Item {AssetId = "assetId1", DescriptionId = 1, Id = 1},
//                new Item {AssetId = "assetId2", DescriptionId = 1, Id = 2},
//                new Item {AssetId = "assetId3", DescriptionId = 1, Id = 3},
//                new Item {AssetId = "assetId4", DescriptionId = 5, Id = 4},
//                new Item {AssetId = "assetId8", DescriptionId = 5, Id = 5},
//            });
//
//            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).Returns(new DatabaseModel.User());
//            
//            var itemService = new ItemService(_fakedRepoService);
//            var res = await itemService.TransferItemsAsync(fromUser, toSteamId, itemsToTransfer);
//
//            Assert.True(res);
//
//            A.CallTo(() => _fakedRepoService.ItemRepoService.ChangeOwner(
//                A<List<int>>.That.Matches(i => i.Contains(1) && i.Contains(3) && i.Contains(4) && i.Contains(5)),
//                A<DatabaseModel.User>.That.Matches(u => u.Id == 1))
//            ).MustHaveHappened();
//        }
//
//
//        [Fact]
//        public async void TransferItemsFailsDueToItemsIsNotTheOwserts()
//        {
//            var fromUser = new DatabaseModel.User
//            {
//                Id = 1
//            };
//            
//            var toSteamId = "toSteamId";
//
//            var itemsToTransfer = new List<AssetAndDescriptionId>
//            {
//                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1},
//                new AssetAndDescriptionId {AssetId = "assetId3", DescriptionId = 1},
//                new AssetAndDescriptionId {AssetId = "assetId4", DescriptionId = 5},
//                new AssetAndDescriptionId {AssetId = "assetId8", DescriptionId = 5},
//            };
//
//            var fakedItemService = A.Fake<IItemService>();
//            A.CallTo(() => fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>
//            {
//                new Item {AssetId = "assetId2", DescriptionId = 1, Id = 2},
//                new Item {AssetId = "assetId3", DescriptionId = 1, Id = 3},
//                new Item {AssetId = "assetId4", DescriptionId = 5, Id = 4},
//                new Item {AssetId = "assetId8", DescriptionId = 5, Id = 5},
//            });
//
//            var itemService = new ItemService(_fakedRepoService);
//            var res = await itemService.TransferItemsAsync(fromUser, toSteamId, itemsToTransfer);
//
//            Assert.False(res);
//            A.CallTo(() => _fakedRepoService.ItemRepoService.ChangeOwner(A<List<int>>._,A<DatabaseModel.User>._)).MustNotHaveHappened();
//
//        }
//

        [Fact]
        public async void TestForBetsJackpotAndOnCoinFlip()
        {
            var user = new DatabaseModel.User("steamId", "name", "imageUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 9);

            A.CallTo(() => _itemRepoService.FindAsync(A<DatabaseModel.User>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, -1, 9, DateTimeOffset.Now, 10),
                new DatabaseModel.Item("assetId2", 1, -1, 9, DateTimeOffset.Now, 11),
                new DatabaseModel.Item("assetId3", 2, -1, 9, DateTimeOffset.Now, 12),
                new DatabaseModel.Item("assetId4", 3, -1, 9, DateTimeOffset.Now, 13),
                new DatabaseModel.Item("assetId5", 3, -1, 9, DateTimeOffset.Now, 13),
                new DatabaseModel.Item("assetId6", 3, -1, 9, DateTimeOffset.Now, 13),
            });
            A.CallTo(() => _descriptionRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("itemDesc1", 1, "730", "2", "img1", true, 1),
                new DatabaseModel.ItemDescription("itemDesc2", 1, "730", "2", "img1", true, 2),
                new DatabaseModel.ItemDescription("itemDesc3", 1, "730", "2", "img1", true, 3)
            });

            var match = new DatabaseModel.Match(1, "salt", "hash", 14.4.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1, DateTime.Now, 1);
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(10, 1, 1, DateTime.Now, 11),
                new DatabaseModel.Bet(9, 1, 2, DateTime.Now, 11)
            };

            A.CallTo(() => _itemBettedRepoSerivce.FindAsync(A<List<DatabaseModel.Bet>>._)).Returns(new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(bets[0].Id, 1, "assetId1", 1),
                new DatabaseModel.ItemBetted(bets[0].Id, 3, "assetId4", 1),
            });

            A.CallTo(() => _matchRepoService.GetCurrentMatch()).Returns(match);
            A.CallTo(() => _betRepoService.FindAsync(A<List<LookUpGameModeBet>>._)).Returns(bets);
            A.CallTo(() => _fakedCoinFlipService.FindAllNotClosedMatches()).Returns(new List<DatabaseModel.CoinFlip>
            {
                new DatabaseModel.CoinFlip
                {
                    Created = DateTime.Today,
                    CreatorIsHead = false,
                    CreatorUserId = 9,
                    GameModeId = 1,
                    Hash = "hash",
                    Salt = "salt",
                    Id = 1,
                    Percentage = "",
                    RoundId = "",
                    SettingId = 0,
                    Status = 1,
                    TimerStarted = null,
                    WinnerId = null
                },
                new DatabaseModel.CoinFlip
                {
                    Created = DateTime.Today,
                    CreatorIsHead = false,
                    CreatorUserId = 10,
                    GameModeId = 1,
                    Hash = "hash",
                    Salt = "salt",
                    Id = 1,
                    Percentage = "",
                    RoundId = "",
                    SettingId = 0,
                    Status = 1,
                    TimerStarted = null,
                    WinnerId = null
                }
            });

            var itemService = new ItemService(_fakedRepoService);

            var avalibleItemsForUser = await itemService.GetAvalibleItemsForUser(user);

            A.CallTo(() => _fakedCoinFlipService.FindAllNotClosedMatches()).MustHaveHappened();

            Assert.Equal(4, avalibleItemsForUser.Count);
            Assert.Equal("itemDesc3", avalibleItemsForUser.Single(item => item.AssetId == "assetId5").Name);
            Assert.Equal("itemDesc3", avalibleItemsForUser.Single(item => item.AssetId == "assetId6").Name);
            Assert.Equal("itemDesc2", avalibleItemsForUser.Single(item => item.AssetId == "assetId3").Name);
            Assert.Equal("itemDesc1", avalibleItemsForUser.Single(item => item.AssetId == "assetId2").Name);
        }

        [Fact]
        public async void GetAcalibleItemsWhenThereAreActiveBetsButNotOurUserSuccess()
        {
            var user = new DatabaseModel.User("steamId", "name", "imageUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 9);

            A.CallTo(() => _itemRepoService.FindAsync(A<DatabaseModel.User>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, -1, 9, DateTimeOffset.Now, 10),
                new DatabaseModel.Item("assetId2", 1, -1, 9, DateTimeOffset.Now, 11),
                new DatabaseModel.Item("assetId3", 2, -1, 9, DateTimeOffset.Now, 12),
                new DatabaseModel.Item("assetId4", 3, -1, 9, DateTimeOffset.Now, 13)
            });
            A.CallTo(() => _descriptionRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("itemDesc1", 1, "730", "2", "img1", true, 1),
                new DatabaseModel.ItemDescription("itemDesc2", 1, "730", "2", "img1", true, 2),
                new DatabaseModel.ItemDescription("itemDesc3", 1, "730", "2", "img1", true, 3)
            });

            var match = new DatabaseModel.Match(1, "salt", "hash", 14.4.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1, DateTime.Now, 1);
            var bets = new List<DatabaseModel.Bet> {new DatabaseModel.Bet(10, 1, 1, DateTime.Now, 11)};

            A.CallTo(() => _matchRepoService.GetCurrentMatch()).Returns(match);
            A.CallTo(() => _betRepoService.FindAsync(match)).Returns(bets);
            var itemService = new ItemService(_fakedRepoService);

            var avalibleItemsForUser = await itemService.GetAvalibleItemsForUser(user);

            Assert.Equal(4, avalibleItemsForUser.Count);

            Assert.Equal("itemDesc1", avalibleItemsForUser.Single(item => item.AssetId == "assetId1").Name);
            Assert.Equal("itemDesc1", avalibleItemsForUser.Single(item => item.AssetId == "assetId2").Name);
            Assert.Equal("itemDesc2", avalibleItemsForUser.Single(item => item.AssetId == "assetId3").Name);
            Assert.Equal("itemDesc3", avalibleItemsForUser.Single(item => item.AssetId == "assetId4").Name);
        }

        [Fact]
        public async void GetAvalibeItemsSuccess()
        {
            var user = new DatabaseModel.User("steamId", "name", "imageUrl", "tradelink", DateTime.Now, DateTime.Now, false);

            var match = new DatabaseModel.Match(1, "salt", "hash", 14.4.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1, DateTime.Now, 1);
            A.CallTo(() => _matchRepoService.GetCurrentMatch()).Returns(match);
            A.CallTo(() => _betRepoService.FindAsync(A<DatabaseModel.Match>._)).Returns(new List<DatabaseModel.Bet>());
            A.CallTo(() => _offerTransactionRepoService.FindActiveAsync(A<DatabaseModel.User>._)).Returns(new List<DatabaseModel.OfferTransaction>());

            A.CallTo(() => _itemRepoService.FindAsync(A<DatabaseModel.User>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 2, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 3, -1, -1, DateTimeOffset.Now)
            });
            A.CallTo(() => _descriptionRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("itemDesc1", 1, "730", "2", "img1", true, 1),
                new DatabaseModel.ItemDescription("itemDesc2", 1, "730", "2", "img1", true, 2),
                new DatabaseModel.ItemDescription("itemDesc3", 1, "730", "2", "img1", true, 3)
            });

            var itemService = new ItemService(_fakedRepoService);

            var avalibleItemsForUser = await itemService.GetAvalibleItemsForUser(user);


            Assert.Equal(4, avalibleItemsForUser.Count);

            Assert.Equal("itemDesc1", avalibleItemsForUser.Single(item => item.AssetId == "assetId1").Name);
            Assert.Equal("itemDesc1", avalibleItemsForUser.Single(item => item.AssetId == "assetId2").Name);
            Assert.Equal("itemDesc2", avalibleItemsForUser.Single(item => item.AssetId == "assetId3").Name);
            Assert.Equal("itemDesc3", avalibleItemsForUser.Single(item => item.AssetId == "assetId4").Name);
        }

        [Fact]
        public async void GetAvalibeItemsWhenUserHasActiveOfferTransactionSuccess()
        {
            var user = new DatabaseModel.User("steamId", "name", "imageUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            var match = new DatabaseModel.Match(1, "salt", "hash", 14.4.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1, DateTime.Now, 1);
            A.CallTo(() => _matchRepoService.GetCurrentMatch()).Returns(match);
            A.CallTo(() => _offerTransactionRepoService.FindActiveAsync(A<DatabaseModel.User>._)).Returns(new List<DatabaseModel.OfferTransaction>
            {
                new DatabaseModel.OfferTransaction(10, 1, new decimal(15), false, "someSteamId", null, 5)
            });
            var itemsInActiveOffer = new List<DatabaseModel.ItemInOfferTransaction>
            {
                new DatabaseModel.ItemInOfferTransaction(5, 1, "assetId1", new decimal(1)),
                new DatabaseModel.ItemInOfferTransaction(5, 2, "assetId3", new decimal(1)),
            };
            A.CallTo(() => _itemsInOfferTransactionRepoService.FindAsync(A<List<DatabaseModel.OfferTransaction>>.That.Matches(
                    list => list.Count == 1 && list[0].Id == 5)))
                .Returns(itemsInActiveOffer);

            A.CallTo(() => _itemRepoService.FindAsync(A<DatabaseModel.User>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 2, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 3, -1, -1, DateTimeOffset.Now)
            });
            A.CallTo(() => _descriptionRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("itemDesc1", 1, "730", "2", "img1", true, 1),
                new DatabaseModel.ItemDescription("itemDesc2", 1, "730", "2", "img1", true, 2),
                new DatabaseModel.ItemDescription("itemDesc3", 1, "730", "2", "img1", true, 3)
            });

            var itemService = new ItemService(_fakedRepoService);

            var avalibleItemsForUser = await itemService.GetAvalibleItemsForUser(user);


            Assert.Equal(2, avalibleItemsForUser.Count);

            Assert.Equal("itemDesc1", avalibleItemsForUser.Single(item => item.AssetId == "assetId2").Name);
            Assert.Equal("itemDesc3", avalibleItemsForUser.Single(item => item.AssetId == "assetId4").Name);
        }

        [Fact]
        public async void GetAvalibeItemsWhenUserHasActiveOfferTransactionAndActiveBetSuccess()
        {
            var user = new DatabaseModel.User("steamId", "name", "imageUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            //setup items
            A.CallTo(() => _itemRepoService.FindAsync(A<DatabaseModel.User>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, -1, 10, DateTimeOffset.Now, 10),
                new DatabaseModel.Item("assetId2", 1, -1, 10, DateTimeOffset.Now, 11),
                new DatabaseModel.Item("assetId3", 2, -1, 10, DateTimeOffset.Now, 12),
                new DatabaseModel.Item("assetId4", 3, -1, 10, DateTimeOffset.Now, 13),
                new DatabaseModel.Item("assetId5", 3, -1, 10, DateTimeOffset.Now, 14),
                new DatabaseModel.Item("assetId6", 3, -1, 10, DateTimeOffset.Now, 15)
            });
            A.CallTo(() => _descriptionRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("itemDesc1", 1, "730", "2", "img1", true, 1),
                new DatabaseModel.ItemDescription("itemDesc2", 1, "730", "2", "img1", true, 2),
                new DatabaseModel.ItemDescription("itemDesc3", 1, "730", "2", "img1", true, 3)
            });

            //setup active bet
            var match = new DatabaseModel.Match(1, "salt", "hash", 14.4.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1, DateTime.Now, 1);
            var bets = new List<DatabaseModel.Bet> {new DatabaseModel.Bet(10, 1, 1, DateTime.Now, 11)};
            A.CallTo(() => _matchRepoService.GetCurrentMatch()).Returns(match);
            A.CallTo(() => _betRepoService.FindAsync(A<List<LookUpGameModeBet>>._)).Returns(bets);
            A.CallTo(() => _itemBettedRepoSerivce.FindAsync(A<List<DatabaseModel.Bet>>._)).Returns(new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(bets[0].Id, 1, "assetId1", 1),
                new DatabaseModel.ItemBetted(bets[0].Id, 3, "assetId4", 1),
            });
            //setup active offer
            A.CallTo(() => _offerTransactionRepoService.FindActiveAsync(A<DatabaseModel.User>._)).Returns(new List<DatabaseModel.OfferTransaction>
            {
                new DatabaseModel.OfferTransaction(10, 1, new decimal(15), false, "someSteamId", null, 5)
            });
            var itemsInActiveOffer = new List<DatabaseModel.ItemInOfferTransaction>
            {
                new DatabaseModel.ItemInOfferTransaction(5, 1, "assetId2", new decimal(1)),
                new DatabaseModel.ItemInOfferTransaction(5, 2, "assetId3", new decimal(1)),
            };
            A.CallTo(() => _itemsInOfferTransactionRepoService.FindAsync(A<List<DatabaseModel.OfferTransaction>>.That.Matches(
                    list => list.Count == 1 && list[0].Id == 5)))
                .Returns(itemsInActiveOffer);

            var itemService = new ItemService(_fakedRepoService);

            var avalibleItemsForUser = await itemService.GetAvalibleItemsForUser(user);


            Assert.Equal(2, avalibleItemsForUser.Count);

            Assert.Equal("itemDesc3", avalibleItemsForUser.Single(item => item.AssetId == "assetId5").Name);
            Assert.Equal("itemDesc3", avalibleItemsForUser.Single(item => item.AssetId == "assetId6").Name);
        }

        [Fact]
        public async void GetAvalibeItemsWhenUserHasActiveBetSuccess()
        {
            var user = new DatabaseModel.User("steamId", "name", "imageUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);


            A.CallTo(() => _itemRepoService.FindAsync(A<DatabaseModel.User>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, -1, -1, DateTimeOffset.Now, 10),
                new DatabaseModel.Item("assetId2", 1, -1, -1, DateTimeOffset.Now, 11),
                new DatabaseModel.Item("assetId3", 2, -1, -1, DateTimeOffset.Now, 12),
                new DatabaseModel.Item("assetId4", 3, -1, -1, DateTimeOffset.Now, 13)
            });
            A.CallTo(() => _descriptionRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("itemDesc1", 1, "730", "2", "img1", true, 1),
                new DatabaseModel.ItemDescription("itemDesc2", 1, "730", "2", "img1", true, 2),
                new DatabaseModel.ItemDescription("itemDesc3", 1, "730", "2", "img1", true, 3)
            });

            var match = new DatabaseModel.Match(1, "salt", "hash", 14.4.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1, DateTime.Now, 1);
            var bets = new List<DatabaseModel.Bet> {new DatabaseModel.Bet(10, 1, 1, DateTime.Now, 11)};

            A.CallTo(() => _matchRepoService.GetCurrentMatch()).Returns(match);
            A.CallTo(() => _betRepoService.FindAsync(A<List<LookUpGameModeBet>>._)).Returns(bets);
            A.CallTo(() => _itemBettedRepoSerivce.FindAsync(A<List<DatabaseModel.Bet>>._)).Returns(new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(bets[0].Id, 1, "assetId1", 1),
                new DatabaseModel.ItemBetted(bets[0].Id, 3, "assetId4", 1),
            });

            var itemService = new ItemService(_fakedRepoService);

            var avalibleItemsForUser = await itemService.GetAvalibleItemsForUser(user);


            Assert.Equal(2, avalibleItemsForUser.Count);

            Assert.Equal("itemDesc1", avalibleItemsForUser.Single(item => item.AssetId == "assetId2").Name);
            Assert.Equal("itemDesc2", avalibleItemsForUser.Single(item => item.AssetId == "assetId3").Name);
        }

        [Fact]
        public async void GetSumOfItems()
        {
            var items = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, -1, -1, DateTimeOffset.Now, 10),
                new DatabaseModel.Item("assetId2", 1, -1, -1, DateTimeOffset.Now, 11),
                new DatabaseModel.Item("assetId3", 2, -1, -1, DateTimeOffset.Now, 12),
                new DatabaseModel.Item("assetId4", 3, -1, -1, DateTimeOffset.Now, 13),
                new DatabaseModel.Item("assetId5", 2, -1, -1, DateTimeOffset.Now, 14)
            };


            A.CallTo(() => _descriptionRepoService.ValueOfItemDescriptions(A<List<int>>._)).Returns(new Dictionary<int, decimal>
            {
                {1, 1},
                {2, 2},
                {3, 4},
            });

            var itemService = new ItemService(_fakedRepoService);

            var res = await itemService.GetSumOfItems(items);


            Assert.Equal(10, res);
        }

        [Fact]
        public async void GetSumOfItems1()
        {
            var items = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, -1, -1, DateTimeOffset.Now, 10),
                new DatabaseModel.Item("assetId2", 1, -1, -1, DateTimeOffset.Now, 11),
                new DatabaseModel.Item("assetId3", 2, -1, -1, DateTimeOffset.Now, 12),
                new DatabaseModel.Item("assetId4", 3, -1, -1, DateTimeOffset.Now, 13),
                new DatabaseModel.Item("assetId5", 2, -1, -1, DateTimeOffset.Now, 14)
            };


            A.CallTo(() => _descriptionRepoService.ValueOfItemDescriptions(A<List<int>>._)).Returns(new Dictionary<int, decimal>
            {
                {1, new decimal(1.45)},
                {2, new decimal(5.07)},
                {3, new decimal(7.3)},
            });

            var itemService = new ItemService(_fakedRepoService);

            var res = await itemService.GetSumOfItems(items);


            Assert.Equal(new decimal(20.34), res);
        }
    }
}