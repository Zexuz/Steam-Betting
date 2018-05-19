using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Backend.Websockets.Models;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;

namespace Betting.Backend.Test
{
    public class BetServiceTest
    {
        private readonly IMatchRepoService           _fakedMatchRepoService;
        private readonly IItemService                _fakedItemService;
        private readonly IUserRepoService            _fakedUserRepoService;
        private readonly IItemRepoService            _fakedItemRepoService;
        private readonly ITransactionWrapper         _fakedTransactionWrapper;
        private readonly ITransactionFactory         _fakedTransactionFactory;
        private readonly IRepoServiceFactory         _fakedRepoServiceFactory;
        private readonly IItemBettedRepoService      _fakedItemBettedRepoService;
        private readonly IBetRepoService             _fakedBetRepoService;
        private readonly IItemDescriptionRepoService _fakedItemDescriptionRepoService;
        private          IMatchHubConnections        _fakedMatchHub;
        private          DatabaseModel.GameMode      _gameMode;
        private          DatabaseModel.Match         _match;
        private          JackpotMatchSetting         _defaultJackpotSetting;

        //The thing to test here is all the "throws" statment and also the transaction. we can't really test anything else atm...

        public BetServiceTest()
        {
            _fakedMatchRepoService = A.Fake<IMatchRepoService>();
            _fakedItemRepoService = A.Fake<IItemRepoService>();
            _fakedUserRepoService = A.Fake<IUserRepoService>();
            _fakedItemService = A.Fake<IItemService>();
            _fakedTransactionWrapper = A.Fake<ITransactionWrapper>();
            _fakedTransactionFactory = A.Fake<ITransactionFactory>();
            _fakedRepoServiceFactory = A.Fake<IRepoServiceFactory>();
            _fakedItemBettedRepoService = A.Fake<IItemBettedRepoService>();
            _fakedBetRepoService = A.Fake<IBetRepoService>();
            _fakedMatchHub = A.Fake<IMatchHubConnections>();
            _fakedItemDescriptionRepoService = A.Fake<IItemDescriptionRepoService>();

            A.CallTo(() => _fakedRepoServiceFactory.MatchRepoService).Returns(_fakedMatchRepoService);
            A.CallTo(() => _fakedRepoServiceFactory.ItemRepoService).Returns(_fakedItemRepoService);
            A.CallTo(() => _fakedRepoServiceFactory.UserRepoService).Returns(_fakedUserRepoService);
            A.CallTo(() => _fakedRepoServiceFactory.ItemDescriptionRepoService).Returns(_fakedItemDescriptionRepoService);

            _gameMode = new DatabaseModel.GameMode
            {
                CurrentSettingId = 1,
                Id = 10,
                IsEnabled = true,
                Type = "Jackpot"
            };

            _defaultJackpotSetting = new JackpotMatchSetting(10, TimeSpan.Zero, 20, 10, 0, 9999, 0, true, false, TimeSpan.Zero, "");

            _match = new DatabaseModel.Match(1, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1, DateTime.Now);
        }


        [Theory]
        [InlineData(10)]
        [InlineData(9)]
        public async void UserBetsOnOpenCoinFlipMatchSuccess(int value)
        {
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            var bettedItems = new List<Item>
            {
                new Item
                {
                    AssetId = "asset1",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset2",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset3",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset4",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
            };
            var listOfItems = bettedItems.Select(item => new DatabaseModel.Item(item.AssetId, 0, 0, 10,DateTimeOffset.Now)).ToList();
            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.CoinFlip
            {
                Id = matchId,
                Salt = "salt",
                Hash = "hash",
                Percentage = 10.ToString(CultureInfo.InvariantCulture),
                Status = status,
                CreatorUserId = 10,
                GameModeId = 5,
                Created = DateTime.Now,
                CreatorIsHead = true,
                RoundId = Guid.NewGuid().ToString(),
                SettingId = 1,
                TimerStarted = null,
                WinnerId = null
            };
            var itemsToBet = listOfItems.Select(item => new AssetAndDescriptionId
            {
                AssetId = item.AssetId,
                DescriptionId = item.DescriptionId,
            }).ToList();

            var itemDescList = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("xD", 2, "730", "2", "",true)
            };
            
            A.CallTo(() => _fakedUserRepoService.FindAsync(ownerSteamId)).Returns(user);
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(user)).Returns(bettedItems);
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand<int>(A<SqlQuery>._)).ReturnsNextFromSequence(1, 2);
            A.CallTo(() => _fakedTransactionFactory.BeginTransaction()).ReturnsNextFromSequence(_fakedTransactionWrapper);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(Task.FromResult(listOfItems));
            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).Returns(value);
            A.CallTo(() => _fakedItemDescriptionRepoService.FindAsync(A<List<int>>._)).Returns(itemDescList);


            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                new BetRepoService(A.Dummy<IDatabaseConnectionFactory>(), new BetQueries()),
                new ItemBettedRepoService(A.Dummy<IDatabaseConnectionFactory>(), new ItemBetQueries()),
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackSetting = new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 4, 4, 10, 9, true, false, TimeSpan.FromSeconds(10), "");

            await betService.PlaceBetOnCoinFlipMatch(match, jackSetting, _gameMode, itemsToBet.Count, listOfItems, user,itemDescList);

            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand<int>(A<SqlQuery>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand(A<SqlQuery>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustHaveHappened();
        }

        [Theory]
        [InlineData(10)]
        [InlineData(9)]
        public async void UserBetsOnOpenCoinFlipMatchWithBothCsgoAndPubgSuccess(int value)
        {
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            var bettedItems = new List<Item>
            {
                new Item
                {
                    AssetId = "asset1",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset2",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset3",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset4",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
            };
            var listOfItems = bettedItems.Select(item => new DatabaseModel.Item(item.AssetId, 0, 0, 10, DateTimeOffset.Now)).ToList();
            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.CoinFlip
            {
                Id = matchId,
                Salt = "salt",
                Hash = "hash",
                Percentage = 10.ToString(CultureInfo.InvariantCulture),
                Status = status,
                CreatorUserId = 10,
                GameModeId = 5,
                Created = DateTime.Now,
                CreatorIsHead = true,
                RoundId = Guid.NewGuid().ToString(),
                SettingId = 1,
                TimerStarted = null,
                WinnerId = null
            };
            var itemsToBet = listOfItems.Select(item => new AssetAndDescriptionId
            {
                AssetId = item.AssetId,
                DescriptionId = item.DescriptionId,
            }).ToList();
            
            
            var itemDescList = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("xD", 2, "730", "2", "",true),
                new DatabaseModel.ItemDescription("xD", 2, "578080", "2", "",true)
            };

            A.CallTo(() => _fakedUserRepoService.FindAsync(ownerSteamId)).Returns(user);
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(user)).Returns(bettedItems);
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand<int>(A<SqlQuery>._)).ReturnsNextFromSequence(1, 2);
            A.CallTo(() => _fakedTransactionFactory.BeginTransaction()).ReturnsNextFromSequence(_fakedTransactionWrapper);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(Task.FromResult(listOfItems));
            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).Returns(value);
            A.CallTo(() => _fakedItemDescriptionRepoService.FindAsync(A<List<int>>._)).Returns(itemDescList);


            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                new BetRepoService(A.Dummy<IDatabaseConnectionFactory>(), new BetQueries()),
                new ItemBettedRepoService(A.Dummy<IDatabaseConnectionFactory>(), new ItemBetQueries()),
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackSetting = new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 4, 4, 10, 9, true, true, TimeSpan.FromSeconds(10), "");

            await betService.PlaceBetOnCoinFlipMatch(match, jackSetting, _gameMode, itemsToBet.Count, listOfItems, user,itemDescList);

            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand<int>(A<SqlQuery>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand(A<SqlQuery>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustHaveHappened();
        }


        [Theory]
        [InlineData(10)]
        [InlineData(9)]
        public async void UserBetsOnOpenMatchSuccess(int value)
        {
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            var bettedItems = new List<Item>
            {
                new Item
                {
                    AssetId = "asset1",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset2",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset3",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset4",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
            };
            var listOfItems = bettedItems.Select(item => new DatabaseModel.Item(item.AssetId, 0, 0, 10, DateTimeOffset.Now)).ToList();
            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.Match(matchId, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), status, null, null, 1, 1,
                DateTime.Now);
            var itemsToBet = listOfItems.Select(item => new AssetAndDescriptionId
            {
                AssetId = item.AssetId,
                DescriptionId = item.DescriptionId,
            }).ToList();

            A.CallTo(() => _fakedMatchRepoService.FindAsync(matchId)).Returns(match);
            A.CallTo(() => _fakedUserRepoService.FindAsync(ownerSteamId)).Returns(user);
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(user)).Returns(bettedItems);
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand<int>(A<SqlQuery>._)).ReturnsNextFromSequence(1, 2);
            A.CallTo(() => _fakedTransactionFactory.BeginTransaction()).ReturnsNextFromSequence(_fakedTransactionWrapper);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(Task.FromResult(listOfItems));
            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).Returns(value);
            A.CallTo(() => _fakedItemDescriptionRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("xD", 2, "730", "2", "",true)
            });


            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                new BetRepoService(A.Dummy<IDatabaseConnectionFactory>(), new BetQueries()),
                new ItemBettedRepoService(A.Dummy<IDatabaseConnectionFactory>(), new ItemBetQueries()),
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackSetting = new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 4, 4, 10, 9, true, false, TimeSpan.FromSeconds(10), "");

            await betService.PlaceBetOnJackpotMatch(match, jackSetting, _gameMode, itemsToBet, ownerSteamId);

            A.CallTo(() => _fakedItemDescriptionRepoService.FindAsync(A<List<int>>._)).MustHaveHappened();
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand<int>(A<SqlQuery>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand(A<SqlQuery>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustHaveHappened();
        }

        [Fact]
        public async void UserBetsItemsThatDoesNotExistThrows()
        {
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>());

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            await Assert.ThrowsAsync<InvalidAssetAndDecriptionIdException>(async () =>
                await betService.PlaceBetOnJackpotMatch(_match, _defaultJackpotSetting, _gameMode, new List<AssetAndDescriptionId>(), null));

            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserBetsPubgItemOnMatchThatDoesNotAcceptThemThrows()
        {
            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackpotSetting = new JackpotMatchSetting(10, TimeSpan.Zero, 20, 10, 0, 9999, 0, true, false, TimeSpan.Zero, "");

            var itemList = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId2", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId3", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId4", DescriptionId = 1},
            };

            var itemDescriptions = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("pubgItem", 4, "578080", "2", "imgUrl",true)
            };

            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(Task.FromResult(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 1, 1, 1, DateTimeOffset.Now),
            }));
            A.CallTo(() => _fakedItemDescriptionRepoService.FindAsync(A<List<int>>._)).Returns(itemDescriptions);

            await Assert.ThrowsAsync<NotAllowedAppIdOnMatchException>(async () =>
                await betService.PlaceBetOnJackpotMatch(_match, jackpotSetting, _gameMode, itemList, null));

            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserBetsCsgoItemOnMatchThatDoesNotAcceptThemThrows()
        {
            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackpotSetting = new JackpotMatchSetting(10, TimeSpan.Zero, 20, 10, 0, 9999, 0, false, true, TimeSpan.Zero, "");

            var itemList = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId2", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId3", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId4", DescriptionId = 1},
            };

            var itemDescriptions = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("pubgItem", 4, "730", "2", "imgUrl",true)
            };

            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(Task.FromResult(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 1, 1, 1, DateTimeOffset.Now),
            }));
            A.CallTo(() => _fakedItemDescriptionRepoService.FindAsync(A<List<int>>._)).Returns(itemDescriptions);

            await Assert.ThrowsAsync<NotAllowedAppIdOnMatchException>(async () =>
                await betService.PlaceBetOnJackpotMatch(_match, jackpotSetting, _gameMode, itemList, null));

            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserBetsItemThatDoesNotExistThrows()
        {
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>
            {
                new Item
                {
                    AssetId = "qwe",
                }
            });

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            await Assert.ThrowsAsync<InvalidAssetAndDecriptionIdException>(async () => await betService.PlaceBetOnJackpotMatch(_match,
                _defaultJackpotSetting,
                _gameMode,
                new List<AssetAndDescriptionId>
                {
                    new AssetAndDescriptionId
                    {
                        AssetId = "asd",
                        DescriptionId = 1
                    }
                }, null));

            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserThatDoesNotExistBetsThrows()
        {
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>
            {
                new Item
                {
                    AssetId = "qwe",
                    DescriptionId = 1
                }
            });
            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).Returns(Task.FromResult<DatabaseModel.User>(null));

            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("qwe", 1, 1, 1, DateTimeOffset.Now)
            });

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            await Assert.ThrowsAsync<UserDoesNotExistException>(async () => await betService.PlaceBetOnJackpotMatch(_match,
                _defaultJackpotSetting,
                _gameMode,
                new List<AssetAndDescriptionId>
                {
                    new AssetAndDescriptionId
                    {
                        AssetId = "asd",
                        DescriptionId = 1
                    }
                }, null));

            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserBetsItemThatHeDoesNotOwn()
        {
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>
            {
                new Item
                {
                    AssetId = "qwe",
                    DescriptionId = 1
                }
            });

            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("qwe", 1, 1, 1, DateTimeOffset.Now)
            });

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            await Assert.ThrowsAsync<UserDoesNotOwnThisItemsException>(async () => await betService.PlaceBetOnJackpotMatch(_match,
                _defaultJackpotSetting,
                _gameMode,
                new List<AssetAndDescriptionId>
                {
                    new AssetAndDescriptionId
                    {
                        AssetId = "asd",
                        DescriptionId = 1
                    }
                }, null));

            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserBetsItemThatIsNotAvalibleThrows()
        {
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>());
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 2, 1, 12, DateTimeOffset.Now, -1)
            });
            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._))
                .Returns(new DatabaseModel.User("steamId", "name", "img", "", DateTime.Now, DateTime.Now, false, null, 12));

            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).Returns(10);

            var jackSetting = new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 100, 1, 100, 0, true, false, TimeSpan.FromSeconds(10), "");

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            await Assert.ThrowsAsync<NoAvalibleItemsForBettingException>(async () => await betService.PlaceBetOnJackpotMatch(_match,
                jackSetting,
                _gameMode,
                new List<AssetAndDescriptionId>
                {
                    new AssetAndDescriptionId
                    {
                        AssetId = "asd",
                        DescriptionId = 1
                    }
                }, null));

            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserBetsItemsThatAlreadyIsBettedThrows()
        {
            //GetAvalibleItemsForUser is also tested in ItemServceTest
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            var bettedItems = new List<Item>
            {
                new Item
                {
                    AssetId = "asset1",
                    Owner = new User {SteamId = user.SteamId}
                },
                new Item
                {
                    AssetId = "asset2",
                    Owner = new User {SteamId = user.SteamId}
                },
                new Item
                {
                    AssetId = "asset3",
                    Owner = new User {SteamId = user.SteamId}
                },
                new Item
                {
                    AssetId = "asset4",
                    Owner = new User {SteamId = user.SteamId}
                },
            };
            var listOfItems = bettedItems.Select(item => new DatabaseModel.Item(item.AssetId, 0, 0, 10, DateTimeOffset.Now)).ToList();

            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.Match(matchId, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), status, null, null, 1, 1,
                DateTime.Now);
            var itemsToBet = listOfItems.Select(item => new AssetAndDescriptionId
            {
                AssetId = item.AssetId,
                DescriptionId = item.DescriptionId,
            }).ToList();

            A.CallTo(() => _fakedMatchRepoService.FindAsync(matchId)).Returns(match);
            A.CallTo(() => _fakedUserRepoService.FindAsync(ownerSteamId)).Returns(user);
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(user)).Returns(new List<Item>());
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand<int>(A<SqlQuery>._)).ReturnsNextFromSequence(1, 2);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(listOfItems);
            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).Returns(10);

            var jackSetting = new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 100, 1, 100, 0, true, false, TimeSpan.FromSeconds(10), "");

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            await Assert.ThrowsAsync<NoAvalibleItemsForBettingException>(
                async () => await betService.PlaceBetOnJackpotMatch(match, jackSetting, _gameMode, itemsToBet, ownerSteamId));
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserBetsItemsThatHeDoesNotOwnThrows()
        {
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).Returns(user);
            var items = new List<Item>
            {
                new Item
                {
                    AssetId = "asset1",
                    Owner = new User {SteamId = user.SteamId}
                }
            };

            var listOfItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("asset1", 0, 0, 0, DateTimeOffset.Now, 12)
            };


            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.Match(matchId, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), status, null, null, 1, 1,
                DateTime.Now);
            var itemsToBet = listOfItems.Select(item => new AssetAndDescriptionId
            {
                AssetId = item.AssetId,
                DescriptionId = item.DescriptionId,
            }).ToList();

            A.CallTo(() => _fakedMatchRepoService.FindAsync(matchId)).Returns(match);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(listOfItems);
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).Returns(new List<Item>());

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            await Assert.ThrowsAsync<UserDoesNotOwnThisItemsException>(async () =>
                await betService.PlaceBetOnJackpotMatch(match, _defaultJackpotSetting, _gameMode, itemsToBet, ownerSteamId)
            );

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Theory]
        [InlineData(5)]
        [InlineData(4)]
        public async void UserBetsOnMatchWithToManyItemsThrows(int limit)
        {
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).Returns(user);

            var listOfItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("asset1", 1, 0, 10, DateTimeOffset.Now, 1),
                new DatabaseModel.Item("asset2", 2, 0, 10, DateTimeOffset.Now, 3),
                new DatabaseModel.Item("asset3", 5, 0, 10, DateTimeOffset.Now, 3),
                new DatabaseModel.Item("asset4", 1, 0, 10, DateTimeOffset.Now, 4),
                new DatabaseModel.Item("asset5", 1, 0, 10, DateTimeOffset.Now, 5),
                new DatabaseModel.Item("asset6", 1, 0, 10, DateTimeOffset.Now, 6),
            };
            var itemsToBet = listOfItems.Select(i => new AssetAndDescriptionId {AssetId = i.AssetId, DescriptionId = i.DescriptionId}).ToList();

            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.Match(matchId, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), status, null, null, 1, 1,
                DateTime.Now);

            A.CallTo(() => _fakedMatchRepoService.FindAsync(matchId)).Returns(match);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(listOfItems);

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackpotSettings =
                new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, limit, 1, 500, 1, true, false, TimeSpan.FromSeconds(10), "");

            await Assert.ThrowsAsync<ToManyItemsOnBetException>(async () =>
                await betService.PlaceBetOnJackpotMatch(match, jackpotSettings, _gameMode, itemsToBet, ownerSteamId)
            );

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Theory]
        [InlineData(8)]
        [InlineData(7)]
        public async void UserBetsOnMatchWithToFewItemsThrows(int limit)
        {
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).Returns(user);

            var listOfItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("asset1", 1, 0, 10, DateTimeOffset.Now, 1),
                new DatabaseModel.Item("asset2", 2, 0, 10, DateTimeOffset.Now, 3),
                new DatabaseModel.Item("asset3", 5, 0, 10, DateTimeOffset.Now, 3),
                new DatabaseModel.Item("asset4", 1, 0, 10, DateTimeOffset.Now, 4),
                new DatabaseModel.Item("asset5", 1, 0, 10, DateTimeOffset.Now, 5),
                new DatabaseModel.Item("asset6", 1, 0, 10, DateTimeOffset.Now, 6),
            };
            var itemsToBet = listOfItems.Select(i => new AssetAndDescriptionId {AssetId = i.AssetId, DescriptionId = i.DescriptionId}).ToList();

            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.Match(matchId, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), status, null, null, 1, 1,
                DateTime.Now);

            A.CallTo(() => _fakedMatchRepoService.FindAsync(matchId)).Returns(match);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(listOfItems);

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackpotSettings =
                new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 10, limit, 500, 1, true, false, TimeSpan.FromSeconds(10), "");

            await Assert.ThrowsAsync<ToFewItemsOnBetException>(async () =>
                await betService.PlaceBetOnJackpotMatch(match, jackpotSettings, _gameMode, itemsToBet, ownerSteamId)
            );

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Theory]
        [InlineData(9)]
        [InlineData(10)]
        public async void UserBetsOnMatchWithToMuchValueThrows(int limit)
        {
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).Returns(user);

            var listOfItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("asset1", 1, 0, 10, DateTimeOffset.Now, 1),
                new DatabaseModel.Item("asset2", 2, 0, 10, DateTimeOffset.Now, 3),
                new DatabaseModel.Item("asset3", 5, 0, 10, DateTimeOffset.Now, 3),
                new DatabaseModel.Item("asset4", 1, 0, 10, DateTimeOffset.Now, 4),
                new DatabaseModel.Item("asset5", 1, 0, 10, DateTimeOffset.Now, 5),
                new DatabaseModel.Item("asset6", 1, 0, 10, DateTimeOffset.Now, 6),
            };
            var itemsToBet = listOfItems.Select(i => new AssetAndDescriptionId {AssetId = i.AssetId, DescriptionId = i.DescriptionId}).ToList();

            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.Match(matchId, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), status, null, null, 1, 1,
                DateTime.Now);

            A.CallTo(() => _fakedMatchRepoService.FindAsync(matchId)).Returns(match);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(listOfItems);
            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).Returns(11);

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackpotSettings =
                new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 10, 1, limit, 1, true, false, TimeSpan.FromSeconds(10), "");

            await Assert.ThrowsAsync<ToMuchValueOnBetException>(async () =>
                await betService.PlaceBetOnJackpotMatch(match, jackpotSettings, _gameMode, itemsToBet, ownerSteamId)
            );

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Theory]
        [InlineData(9)]
        [InlineData(10)]
        public async void UserBetsOnMatchWithToLittleValueThrows(int limit)
        {
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).Returns(user);

            var listOfItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("asset1", 1, 0, 10, DateTimeOffset.Now, 1),
                new DatabaseModel.Item("asset2", 2, 0, 10, DateTimeOffset.Now, 3),
                new DatabaseModel.Item("asset3", 5, 0, 10, DateTimeOffset.Now, 3),
                new DatabaseModel.Item("asset4", 1, 0, 10, DateTimeOffset.Now, 4),
                new DatabaseModel.Item("asset5", 1, 0, 10, DateTimeOffset.Now, 5),
                new DatabaseModel.Item("asset6", 1, 0, 10, DateTimeOffset.Now, 6),
            };
            var itemsToBet = listOfItems.Select(i => new AssetAndDescriptionId {AssetId = i.AssetId, DescriptionId = i.DescriptionId}).ToList();

            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.Match(matchId, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), status, null, null, 1, 1,
                DateTime.Now);

            A.CallTo(() => _fakedMatchRepoService.FindAsync(matchId)).Returns(match);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(listOfItems);
            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).Returns(8);

            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                _fakedBetRepoService,
                _fakedItemBettedRepoService,
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackpotSettings =
                new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 10, 1, 100, limit, true, false, TimeSpan.FromSeconds(10), "");

            await Assert.ThrowsAsync<ToLittleValueOnBetException>(async () =>
                await betService.PlaceBetOnJackpotMatch(match, jackpotSettings, _gameMode, itemsToBet, ownerSteamId)
            );

            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserBetsOnDisabledGameModeThrows()
        {
            var matchId = 1;
            var ownerSteamId = "steamId";
            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, false, null, 10);

            var bettedItems = new List<Item>
            {
                new Item
                {
                    AssetId = "asset1",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset2",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset3",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
                new Item
                {
                    AssetId = "asset4",
                    Owner = new User {SteamId = user.SteamId},
                    DescriptionId = 1
                },
            };
            var listOfItems = bettedItems.Select(item => new DatabaseModel.Item(item.AssetId, 0, 0, 10, DateTimeOffset.Now)).ToList();
            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
            var match = new DatabaseModel.Match(matchId, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), status, null, null, 1, 1,
                DateTime.Now);
            var itemsToBet = listOfItems.Select(item => new AssetAndDescriptionId
            {
                AssetId = item.AssetId,
                DescriptionId = item.DescriptionId,
            }).ToList();

            A.CallTo(() => _fakedMatchRepoService.FindAsync(matchId)).Returns(match);
            A.CallTo(() => _fakedUserRepoService.FindAsync(ownerSteamId)).Returns(user);
            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(user)).Returns(bettedItems);
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand<int>(A<SqlQuery>._)).ReturnsNextFromSequence(1, 2);
            A.CallTo(() => _fakedTransactionFactory.BeginTransaction()).ReturnsNextFromSequence(_fakedTransactionWrapper);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(Task.FromResult(listOfItems));
            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).Returns(9);
            A.CallTo(() => _fakedItemDescriptionRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("xD", 2, "730", "2", "",true)
            });


            var betService = new BetService(
                _fakedRepoServiceFactory,
                _fakedItemService,
                _fakedTransactionFactory,
                new BetRepoService(A.Dummy<IDatabaseConnectionFactory>(), new BetQueries()),
                new ItemBettedRepoService(A.Dummy<IDatabaseConnectionFactory>(), new ItemBetQueries()),
                A.Dummy<IDiscordService>(),
                _fakedMatchHub
            );

            var jackSetting = new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 4, 4, 10, 9, true, false, TimeSpan.FromSeconds(10), "");

            _gameMode.IsEnabled = false;

            await Assert.ThrowsAsync<GameModeIsNotEnabledException>(
                async () =>
                    await betService.PlaceBetOnJackpotMatch(match, jackSetting, _gameMode, itemsToBet, ownerSteamId)
            );

            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand<int>(A<SqlQuery>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedTransactionWrapper.ExecuteSqlCommand(A<SqlQuery>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustNotHaveHappened();
            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
        }

//        [Fact]
//        public async void UserBetsOnMatchThatDoesNotAllowThisInventoryThrows()
//        {
//            var matchId = 1;
//            var ownerSteamId = "steamId";
//            var user = new DatabaseModel.User(ownerSteamId, "name", "imgUrl", "tradelink", DateTime.Now, DateTime.Now, null, 10);
//
//            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).Returns(user);
//
//            var listOfItems = new List<DatabaseModel.Item>
//            {
//                new DatabaseModel.Item("asset1", 1, 0, 10, 1),
//                new DatabaseModel.Item("asset2", 2, 0, 10, 3),
//                new DatabaseModel.Item("asset3", 5, 0, 10, 3),
//                new DatabaseModel.Item("asset4", 1, 0, 10, 4),
//                new DatabaseModel.Item("asset5", 1, 0, 10, 5),
//                new DatabaseModel.Item("asset6", 1, 0, 10, 6),
//            };
//            var itemsToBet = listOfItems.Select(i => new AssetAndDescriptionId {AssetId = i.AssetId, DescriptionId = i.DescriptionId}).ToList();
//
//            var status = MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open);
//            var match = new DatabaseModel.Match(matchId, "salt", "hash", 10, status, null, null, 1, 1, DateTime.Now);
//
//            A.CallTo(() => _fakedMatchRepoService.FindAsync(matchId)).Returns(match);
//            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(listOfItems);
//            A.CallTo(() => _fakedItemDescriptionRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
//            {
//                new DatabaseModel.ItemDescription("name1", 0, "730", "2", "", 1),
//                new DatabaseModel.ItemDescription("name2", 0, "730", "2", "", 2),
//                new DatabaseModel.ItemDescription("name5", 0, "730", "5", "", 5),
//            });
//
//            var betService = new BetService(
//                _fakedRepoServiceFactory,
//                _fakedItemService,
//                _fakedTransactionFactory,
//                _fakedBetRepoService,
//                _fakedItemBettedRepoService,
//                _fakedMatchHub
//            );
//
//            var jackpotSettings = new JackpotMatchSetting(10, TimeSpan.FromSeconds(10), 50, 10, 1, 100, 0, TimeSpan.FromSeconds(10), "");
//
//            await Assert.ThrowsAsync<InvalidItemTypeException>(async () =>
//                await betService.PlaceBetOnJackpotMatch(match, jackpotSettings, itemsToBet, ownerSteamId)
//            );
//
//            A.CallTo(() => _fakedItemDescriptionRepoService.FindAsync(A<List<int>>._)).MustHaveHappened();
//            A.CallTo(() => _fakedUserRepoService.FindAsync(A<string>._)).MustHaveHappened();
//            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).MustHaveHappened();
//            A.CallTo(() => _fakedItemService.GetSumOfItems(A<List<DatabaseModel.Item>>._)).MustNotHaveHappened();
//            ;
//            A.CallTo(() => _fakedItemService.GetAvalibleItemsForUser(A<DatabaseModel.User>._)).MustNotHaveHappened();
//            A.CallTo(() => _fakedMatchHub.UserBetsOnMatch(A<UserBetsOnMatchModel>._)).MustNotHaveHappened();
//        }
    }
}